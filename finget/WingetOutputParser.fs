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

    module private InnerParser =

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

        let parseColumn column =
            match column.Length with
            | Some length -> anyString length |>> String.trimEnd
            | None -> restOfLine false

        let parseResultLine columns = plist columns parseColumn

        let colValue columns name valueList =
            let column = columns |> Map.tryFind name

            match column with
            | None -> ""
            | Some column -> valueList |> List.item column.Index |> String.trim

        let parser =
            ((opt skipNewline) >>. parseFirstLine .>> parseDashRow)
            >>= (fun columns ->
                let colValue = (colValue (columns |> Map.ofSeqWithKey (fun column -> column.Name)))

                (sepBy1 (parseResultLine columns) newline)
                |>> List.map (fun values -> {
                    Name = colValue "Name" values
                    PackageId = colValue "Id" values
                    Version = colValue "Version" values
                    Match = colValue "Match" values
                    Source = colValue "Source" values
                })
            )
            .>> eof

    let tryParse =
        String.trim
        >> String.trimStartChars '-'
        >> String.trim
        >> run InnerParser.parser
        >> function
            | Success(result, _, _) -> Result.Ok result
            | Failure(msg, _, _) -> Result.Error msg
