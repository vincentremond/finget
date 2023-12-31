﻿open System
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

// Get currently installed packages
let installedPackages =
    let listCommand =
        "winget.exe",
        [
            "list"
            "--disable-interactivity"
        ]

    displayCommand listCommand

    let exitCode, commandOutput, outputErrors =
        AnsiConsole.status "Getting currently installed packages..." (fun _ -> listCommand |> Command.run)

    if exitCode <> 0 then
        AnsiConsole.MarkupLine $"[red]Error:[/] %s{esc outputErrors}"
        AnsiConsole.Write(Rule("[red]errors[/]"))
        AnsiConsole.WriteLine(commandOutput)
        AnsiConsole.Write(Rule() |> Rule.withStyle Style.grey)
        Environment.Exit(-1)
        Map.empty
    else
        let packages = WingetOutputParser.tryParse InstalledPackage.init commandOutput

        match packages with
        | Error error ->
            AnsiConsole.MarkupLine $"[red]Failed to parse winget output.[/] Error: %s{esc error}\n%s{commandOutput}"
            Environment.Exit(-1)
            Map.empty
        | Ok packages ->
            AnsiConsole.MarkupLine $"Found [yellow]%d{packages.Length}[/] already installed packages"
            packages |> Map.ofSeqWithKey (fun p -> p.PackageId)

let rec loop (installedPackages: Map<string, InstalledPackage>) =
    AnsiConsole.Write(
        Rule(esc (DateTimeOffset.Now.ToString("u")))
        |> Rule.align Justify.Left
        |> Rule.withStyle Style.grey
    )

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
        let packages = WingetOutputParser.tryParse SearchResult.init commandOutput

        match packages with
        | Error error ->
            AnsiConsole.MarkupLine $"[red]Failed to parse winget output.[/] Error: %s{esc error}\n%s{commandOutput}"
        | Ok [] -> AnsiConsole.MarkupLine "[yellow]No packages found[/]"
        | Ok packages' ->
            let packages = packages' |> List.sortBy (fun p -> String.toLower p.PackageId)
            AnsiConsole.MarkupLine $"Found [green]%d{packages.Length}[/] packages"
            AnsiConsole.WriteLine()

            AnsiConsole.Write(
                Table()
                |> Table.addColumns [
                    TableColumn("Id")
                    TableColumn("Name")
                    TableColumn("Version")
                    TableColumn("Match")
                    TableColumn("Source")
                    TableColumn("Installed")

                ]
                |> Table.addRows (
                    packages
                    |> List.map (fun p -> [
                        Text p.PackageId
                        Text p.Name
                        Text p.Version
                        Text p.Match
                        Text p.Source
                        (match installedPackages.TryFind p.PackageId with
                         | Some installedPackage -> Markup($"[green]{esc installedPackage.Version}[/]")
                         | None -> Markup "")
                    ])
                )
            )

            AnsiConsole.WriteLine()

            let selectedPackage =
                AnsiConsole.Prompt(
                    SelectionPrompt<SearchResult option>()
                    |> SelectionPrompt.setTitle "Select a package to install"
                    |> SelectionPrompt.setPageSize 4
                    |> SelectionPrompt.addChoices ((packages |> List.map Some) @ [ None ])
                    |> SelectionPrompt.setWrapAround true
                    |> SelectionPrompt.useConverter (
                        function
                        | Some p -> $"{p.PackageId} (version {p.Version})"
                        | None -> "Cancel"
                    )
                )

            match selectedPackage with
            | None -> ()
            | Some selectedPackage ->

                let installCommand =
                    "winget.exe",
                    [
                        "install"
                        "--id"
                        selectedPackage.PackageId
                        "--source"
                        "winget"
                        "--disable-interactivity"
                        "--wait"
                    ]

                displayCommand installCommand

                let exitCode =
                    AnsiConsole.status
                        $"Installing %s{selectedPackage.PackageId}..."
                        (fun _ -> installCommand |> Command.popup)

                if exitCode <> 0 then
                    AnsiConsole.MarkupLine
                        $"Failed to install [red]%s{esc selectedPackage.PackageId}[/] Exit code : %i{exitCode}"
                else
                    AnsiConsole.MarkupLine $"Successfully installed [green]%s{esc selectedPackage.PackageId}[/]"

                    let newInstalledPackages =
                        installedPackages
                        |> Map.add selectedPackage.PackageId selectedPackage.asInstalledPackage

                    loop newInstalledPackages

    loop installedPackages

loop installedPackages
