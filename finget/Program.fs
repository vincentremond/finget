open System
open Spectre.Console
open finget

let fontSource = FSharp.Data.LiteralProviders.TextFile.``ANSI-Shadow.flf``.Text
let font = FigletFont.Parse(fontSource)

AnsiConsole.Clear()
AnsiConsole.Write(Rule() |> Rule.withStyle Style.red)

AnsiConsole.WriteLine()
AnsiConsole.WriteLine()

AnsiConsole.Write(FigletText(font, "Winget Searcher") |> FigletText.align Justify.Center)

AnsiConsole.Write(Rule() |> Rule.withStyle Style.red)

AnsiConsole.WriteLine()

let esc = Markup.Escape

let implode (strings: string seq) = strings |> String.concat " "

let displayCommand (command: Command) =
    let executable, arguments = command
    AnsiConsole.MarkupLine $"[grey]%s{esc executable}[/] %s{esc (implode arguments)}"

while true do
    AnsiConsole.Write(Rule(esc (DateTimeOffset.Now.ToString("u"))) |> Rule.align Justify.Left |> Rule.withStyle Style.grey)

    let searchFor =
        AnsiConsole.Prompt<string>(
            TextPrompt<string>("What package do you want to install ? ")
            |> TextPrompt.setStyle Style.yellow
        )

    let searchCommand =
        "winget.exe",
        [
            "search"
            "--query"
            searchFor
            "--source"
            "winget"
            "--disable-interactivity"
        ]

    displayCommand searchCommand

    let exitCode, commandOutput, outputErrors =
        AnsiConsole.status $"Searching for [green]%s{esc searchFor}[/] ..." (fun _ -> searchCommand |> Command.run)

    if exitCode <> 0 then
        AnsiConsole.MarkupLine $"[red]Error:[/] %s{esc outputErrors}"
    else
        let packages = WingetOutputParser.tryParse commandOutput

        match packages with
        | Error error -> AnsiConsole.MarkupLine $"[red]Failed to parse winget output.[/] Error: %s{esc error}\n%s{commandOutput}"
        | Ok [] -> AnsiConsole.MarkupLine "[yellow]No packages found[/]"
        | Ok packages ->
            AnsiConsole.MarkupLine $"Found [green]%d{packages.Length}[/] packages"
            AnsiConsole.WriteLine()

            let selectedPackage =
                AnsiConsole.Prompt(
                    SelectionPrompt<SearchResult>()
                    |> SelectionPrompt.setTitle "Select a package to install"
                    |> SelectionPrompt.setPageSize 15
                    |> SelectionPrompt.addChoices packages
                    |> SelectionPrompt.useConverter (fun p -> $"{p.PackageId} (version {p.Version})")
                )

            let installCommand =
                "winget.exe",
                [
                    "install"
                    "--id"
                    selectedPackage.PackageId
                    "--source"
                    "winget"
                    "--disable-interactivity"
                ]

            displayCommand searchCommand

            let exitCode, stdOut, stdErr =
                AnsiConsole.status $"Installing %s{selectedPackage.PackageId}..." (fun _ -> installCommand |> Command.run)

            if exitCode <> 0 then
                AnsiConsole.MarkupLine $"Failed to install [red]%s{esc selectedPackage.PackageId}[/] Error: %s{esc stdErr}"
                AnsiConsole.Write(Rule("[red]errors[/]"))
                AnsiConsole.WriteLine(stdOut)
                AnsiConsole.Write(Rule("output"))
                AnsiConsole.WriteLine(stdOut)
                AnsiConsole.Write(Rule() |> Rule.withStyle Style.grey)
            else
                AnsiConsole.MarkupLine $"Successfully installed [green]%s{esc selectedPackage.PackageId}[/]"
                AnsiConsole.Write(Rule("output") |> Rule.withStyle Style.grey)
                AnsiConsole.WriteLine(stdOut)
                AnsiConsole.Write(Rule() |> Rule.withStyle Style.grey)
