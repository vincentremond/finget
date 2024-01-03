namespace finget

open FParsec

type Column = {
    Name: string
    Index: int
    StartColumn: int
    Length: int option
}

[<RequireQualifiedAccess>]
module WingetOutputParser =
    open System

    module private InnerParser =
        open System

        let someSpaces = many1Chars (pchar ' ')

        let word =
            many1Chars (
                noneOf [
                    ' '
                    '\n'
                    '\r'
                    '\t'
                ]
            )

        let plist (list: 'a list) (parametrizedParser: 'a -> Parser<'b, 'c>) : Parser<'b list, 'c> =
            list
            |> List.map parametrizedParser
            |> List.fold
                (fun state parser -> (state .>>. parser) |>> (fun (state, item) -> state @ [ item ]))
                (preturn [])

        let (!?) p = getPosition .>>. p

        let parseFirstLine: Parser<Column list, unit> =
            (sepBy1 (!?word) someSpaces) .>> newline
            |>> (fun columns ->
                let cols = columns |> List.pairwise'

                cols
                |> List.mapi (fun index ((position, name), next) -> {
                    Name = name
                    Index = index
                    StartColumn = (int position.Column)
                    Length =
                        (next
                         |> Option.map (fun (nextPosition, _) -> int nextPosition.Index - int position.Index))
                })
            )

        let parseDashRow = (many1Chars (pchar '-') .>>. newline) |>> ignore

        // alternative to `anyString` that doesn't consume the newline
        let anyString' length =
            manyMinMaxSatisfy
                0
                length
                (function
                | '\n' -> false
                | _ -> true
                )

        let parseColumn column =
            match column.Length with
            | Some length -> anyString' length |>> String.trimEnd
            | None -> restOfLine false

        let parseResultLine columns = plist columns parseColumn

        let colValue columns valueList name =
            let column = columns |> Map.tryFind name

            match column with
            | None -> ""
            | Some column -> valueList |> List.item column.Index |> String.trim

        let skipBlankLine = manyChars (pchar ' ') .>> newline |>> ignore

        let notLetter = satisfy (fun c -> not (Char.IsLetter c))
        let skipNotLetter = notLetter |>> ignore

        let skipLineNotStartingWithALetter = skipNotLetter >>. skipRestOfLine true

        let skipFuzzyThingsBeforeHeader =
            skipMany (
                choice [
                    attempt skipBlankLine
                    skipLineNotStartingWithALetter
                ]
            )

        let parseHeader = parseFirstLine .>> parseDashRow

        let parser (resultInitializer: PropertyReader -> 'a) =
            skipFuzzyThingsBeforeHeader >>. parseHeader
            >>= (fun columns ->
                let colValue = (colValue (columns |> Map.ofSeqWithKey (fun column -> column.Name)))

                (sepBy1 (parseResultLine columns) newline)
                |>> List.map (fun values -> resultInitializer (colValue values))
            )
            .>> eof
    let tryReplaceUnhandledCharacters input =
        // TODO this is a quick fix, a better solution would be decrease the lenght of the string of 1 for each CJK character.
        input |> Regex.replace "[\u4E00-\u9FFF]" "??" // CJK Unified Ideographs (Chinese, Japanese, Korean) creates problems

    let tryParse (init: PropertyReader -> 'a) : string -> Result<'a list,string> =
        tryReplaceUnhandledCharacters
        >> String.trim
        >> String.trimStartChars '-'
        >> String.trim
        >> run (InnerParser.parser init)
        >> function
            | Success(result, _, _) -> Result.Ok result
            | Failure(msg, _, _) -> Result.Error msg
