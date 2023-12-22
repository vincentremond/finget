[<AutoOpen>]
module finget.Common

[<RequireQualifiedAccess>]
module String =
    let trim (s: string) = s.Trim()
    let trimEnd (s: string) = s.TrimEnd()
    let replace (s: string) (old: string) (``new``: string) = s.Replace(old, ``new``)