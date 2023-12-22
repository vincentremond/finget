open Spectre.Console
open finget

let fontSource = FSharp.Data.LiteralProviders.TextFile.``ANSI-Shadow.flf``.Text
let font = FigletFont.Parse(fontSource)

// Banner
AnsiConsole.Write(
    Rule()
    |> Rule.withStyle Style.red
)

AnsiConsole.WriteLine()

AnsiConsole.Write(
    FigletText(font, "Winget Searcher")
    |> FigletText.align Justify.Center
)

AnsiConsole.Write(
    Rule()
    |> Rule.withStyle Style.red
)

AnsiConsole.WriteLine()

let searchFor = AnsiConsole.Ask<string>("What package do you want to search for? ")

let escapeArgument (argument: string) =
    argument
    |> String.replace "\"" "\\\""
    |> sprintf "\"%s\""

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

let (exitCode, commandOutput, outputErrors) =
    AnsiConsole.status "Searching for packages..." (fun ctx -> searchCommand |> Command.run)

if exitCode <> 0 then
    AnsiConsole.MarkupLine $"[red]Error:[/] %s{outputErrors}"
else
    let packages = WingetOutputParser.tryParse commandOutput

    match packages with
    | Error error -> AnsiConsole.MarkupLine $"[red]Failed to parse winget output.[/] Error: %s{error}"
    | Ok [] -> AnsiConsole.MarkupLine "[yellow]No packages found[/]"
    | Ok packages ->
        AnsiConsole.MarkupLine $"[green]Found %d{packages.Length} packages[/]"
        AnsiConsole.WriteLine()

        let selectedPackage =
            AnsiConsole.Prompt(
                SelectionPrompt<SearchResult>()
                |> SelectionPrompt.setTitle "Select a package to install"
                |> SelectionPrompt.setPageSize 10
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

        let (exitCode, stdOut, StdErr) =
            AnsiConsole.status $"Installing %s{selectedPackage.PackageId}..." (fun ctx -> installCommand |> Command.run)

        if exitCode <> 0 then
            AnsiConsole.MarkupLine $"[red]Error:[/] %s{StdErr}"
        else
            AnsiConsole.MarkupLine $"[green]Successfully installed %s{selectedPackage.PackageId}[/]"
            AnsiConsole.WriteLine(stdOut)
