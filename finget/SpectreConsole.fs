[<AutoOpen>]
module finget.SpectreConsole

open Spectre.Console

[<RequireQualifiedAccess>]
module Style =
    let red = Style.Parse("red")
    let yellow = Style.Parse("yellow")
    let grey = Style.Parse("grey")

[<RequireQualifiedAccess>]
module Rule =
    let withStyle style (rule: Rule) =
        rule.Style <- style
        rule

    let align (justify: Justify) (rule: Rule) =
        rule.Justification <- justify
        rule

[<RequireQualifiedAccess>]
module AnsiConsole =
    let status (text: string) (f: StatusContext -> 'a) : 'a = AnsiConsole.Status().Start(text, f)

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
    let addChoices (choices: 'a list) (prompt: SelectionPrompt<'a>) = prompt.AddChoices(choices)
    // useConverter
    let useConverter (converter: 'a -> string) (prompt: SelectionPrompt<'a>) = prompt.UseConverter(converter)

[<RequireQualifiedAccess>]
module FigletText =
    let align (justify: Justify) (figlet: FigletText) =
        figlet.Justification <- justify
        figlet

[<RequireQualifiedAccess>]
module TextPrompt =
    let setStyle (style: Style) (prompt: TextPrompt<'a>) =
        prompt.PromptStyle <- style
        prompt

[<RequireQualifiedAccess>]
module Table =
    open Spectre.Console.Rendering
    let addColumns (columns: TableColumn seq) (table: Table) = table.AddColumns(Array.ofSeq columns)

    let addRows (rows: IRenderable seq seq) (table: Table) =
        rows |> Seq.iter (fun row -> table.Rows.Add(row) |> ignore)
        table
