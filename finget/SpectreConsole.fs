[<AutoOpen>]
module finget.SpectreConsole

open Spectre.Console

[<RequireQualifiedAccess>]
module Style =
    let red = Style.Parse("red")

[<RequireQualifiedAccess>]
module Rule =
    let withStyle style (rule: Rule) =
        rule.Style <- style
        rule

[<RequireQualifiedAccess>]
module AnsiConsole =
    let status (text: string) (f: StatusContext -> 'a) : 'a =
        AnsiConsole
            .Status()
            .Start(text, f)

[<RequireQualifiedAccess>]
module SelectionPrompt =
    // setTitle
    let setTitle (title: string) (prompt: SelectionPrompt<'a>) =
        prompt.Title <- title
        prompt
    // setPageSize
    let setPageSize (pageSize: int) (prompt: SelectionPrompt<'a>) =
        prompt.PageSize <- pageSize
        prompt
    // addChoices
    let addChoices (choices: 'a list) (prompt: SelectionPrompt<'a>) =
        prompt.AddChoices(choices)
    // useConverter
    let useConverter (converter: 'a -> string) (prompt: SelectionPrompt<'a>) =
        prompt.UseConverter(converter)

[<RequireQualifiedAccess>]
module FigletText =
    let align (justify: Justify) (figlet: FigletText) = 
        figlet.Justification <- justify
        figlet