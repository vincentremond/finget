namespace finget

open FParsec

type ColumnsSize = {
    LengthName: int
    LengthPackageId: int
    LengthVersion: int
    LengthMatch: int
    LengthSource: int
} with

    member this.TotalWidth =
        this.LengthName
        + 1
        + this.LengthPackageId
        + 1
        + this.LengthVersion
        + 1
        + this.LengthMatch
        + 1
        + this.LengthSource

[<RequireQualifiedAccess>]
module WingetOutputParser =

    open FParsec

    module private InnerParser =

        let manySpaces = manyChars (pchar ' ')

        let len str =
            pipe2 (pstring str) manySpaces (fun a b -> a.Length + b.Length)

        let parseFirstLine: Parser<ColumnsSize, unit> =
            (pipe5
                (len "Name")
                (len "Id")
                (len "Version")
                (len "Match")
                (len "Source")
                (fun name id version ``match`` source -> {
                    LengthName = name - 1
                    LengthPackageId = id - 1
                    LengthVersion = version - 1
                    LengthMatch = ``match`` - 1
                    LengthSource = source
                }))

        let parseDashRow (state: ColumnsSize) =
            skipArray state.TotalWidth (skipChar '-')

        let parseResultLine state =
            pipe5
                (anyString state.LengthName
                 .>> skipChar ' ')
                (anyString state.LengthPackageId
                 .>> skipChar ' ')
                (anyString state.LengthVersion
                 .>> skipChar ' ')
                (anyString state.LengthMatch
                 .>> skipChar ' ')
                (anyString state.LengthSource)
                (fun name id version ``match`` source -> {
                    Name = name |> String.trimEnd
                    PackageId = id |> String.trimEnd
                    Version = version |> String.trimEnd
                    Match = ``match`` |> String.trimEnd
                    Source = source |> String.trimEnd
                })

        let parser =
            ((opt skipNewline)
             >>. parseFirstLine
             .>> newline)
            >>= (fun columnsSize ->
                (parseDashRow columnsSize)
                >>. newline
                >>. (sepBy1 (parseResultLine columnsSize) newline)
            )

    let tryParse =
        String.trim
        >> run InnerParser.parser
        >> function
            | Success(result, _, _) -> Result.Ok result
            | Failure(msg, _, _) -> Result.Error msg
