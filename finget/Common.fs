[<AutoOpen>]
module finget.Common

[<RequireQualifiedAccess>]
module String =
    open System.Text
    let trim (s: string) = s.Trim()
    let trimStartChars (char: char) (s: string) = s.TrimStart(char)
    let trimStart (s: string) = s.TrimStart()
    let trimEnd (s: string) = s.TrimEnd()
    let replace (s: string) (old: string) (``new``: string) = s.Replace(old, ``new``)

[<RequireQualifiedAccess>]
module List =
    let pairwise' (list: 'a list) : ('a * 'a option) list =
        let rec loop acc =
            function
            | [] -> acc
            | [ x ] -> (x, None) :: acc
            | x :: y :: rest -> loop ((x, Some y) :: acc) (y :: rest)

        loop [] list |> List.rev

    let fold' folder list state = List.fold folder state list

[<RequireQualifiedAccess>]
module Map =
    let ofSeqWithKey f seq =
        seq |> Seq.map (fun item -> f item, item) |> Map.ofSeq

[<RequireQualifiedAccess>]
module Regex =
    open System.Text.RegularExpressions
    let replace (pattern: string) (replacement: string) (s: string) = Regex.Replace(s, pattern, replacement)
