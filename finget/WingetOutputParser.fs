namespace finget

open FParsec

type ColumnsSize = {
    LengthName: int
    LengthPackageId: int
    LengthVersion: int
    LengthMatch: int
    HasSourceColumn: bool
}

[<RequireQualifiedAccess>]
module WingetOutputParser =

    module private InnerParser =

        let someSpaces = many1Chars (pchar ' ')

        let len str =
            pipe2 (pstring str) someSpaces (fun a b -> a.Length + b.Length)

        let parseFirstLine =

            pipe5
                (pstring "Name" .>>. someSpaces)
                (pstring "Id" .>>. someSpaces)
                (pstring "Version" .>>. someSpaces)
                (pstring "Match")
                ((choice [
                    ((someSpaces .>>. pstring "Source") |>> Some)
                    (preturn None)
                 ])
                 .>> newline)
                (fun (name, nameSpaces) (id, idSpaces) (version, versionSpaces) match_ choice ->
                    let matchSpaces, hasSourceColumn =
                        match choice with
                        | Some(matchSpaces, _) -> matchSpaces.Length - 1, true
                        | None -> 0, false

                    {
                        LengthName = name.Length + nameSpaces.Length - 1
                        LengthPackageId = id.Length + idSpaces.Length - 1
                        LengthVersion = version.Length + versionSpaces.Length - 1
                        LengthMatch = match_.Length + matchSpaces
                        HasSourceColumn = hasSourceColumn
                    }
                )

        let parseDashRow = (many1Chars (pchar '-') .>>. newline) |>> ignore

        let parseResultLine state =
            match state.HasSourceColumn with
            | true ->
                pipe5
                    (anyString state.LengthName .>> skipChar ' ')
                    (anyString state.LengthPackageId .>> skipChar ' ')
                    (anyString state.LengthVersion .>> skipChar ' ')
                    (anyString state.LengthMatch .>> skipChar ' ')
                    (restOfLine false)
                    (fun name id version ``match`` source -> {
                        Name = name |> String.trimEnd
                        PackageId = id |> String.trimEnd
                        Version = version |> String.trimEnd
                        Match = ``match`` |> String.trimEnd
                        Source = source |> String.trimEnd |> Some
                    })
            | false ->
                pipe4
                    (anyString state.LengthName .>> skipChar ' ')
                    (anyString state.LengthPackageId .>> skipChar ' ')
                    (anyString state.LengthVersion .>> skipChar ' ')
                    (restOfLine false)
                    (fun name id version ``match`` -> {
                        Name = name |> String.trimEnd
                        PackageId = id |> String.trimEnd
                        Version = version |> String.trimEnd
                        Match = ``match`` |> String.trimEnd
                        Source = None
                    })

        let parser =
            ((opt skipNewline) >>. parseFirstLine .>> parseDashRow)
            >>= (fun columnsSize -> (sepBy1 (parseResultLine columnsSize) newline))

    let tryParse =
        String.trim
        >> String.trimStartChars '-'
        >> String.trim
        >> run InnerParser.parser
        >> function
            | Success(result, _, _) -> Result.Ok result
            | Failure(msg, _, _) -> Result.Error msg
