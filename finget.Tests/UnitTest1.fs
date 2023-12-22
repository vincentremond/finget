module finget.Tests

open finget
open NUnit.Framework
open Swensen.Unquote

[<SetUp>]
let Setup () = ()

[<Test>]
let Test1 () =
    let wingetOutput =
        """
Name                         Id                                  Version      Match          Source
---------------------------------------------------------------------------------------------------
7-Zip 19.00 (x64)            7zip.7zip                           1.9.0        Moniker: 7zip  winget
Microsoft Visual Studio Code Microsoft.VisualStudioCode          1.54.3       Tag: vscode    winget
Java SE Development Kit 11   Oracle.JDK                          14.1                        winget
"""

    test
        <@
            WingetOutputParser.tryParse wingetOutput = Ok [
                {
                    Name = "7-Zip 19.00 (x64)"
                    PackageId = "7zip.7zip"
                    Version = "1.9.0"
                    Match = "Moniker: 7zip"
                    Source = "winget"
                }
                {
                    Name = "Microsoft Visual Studio Code"
                    PackageId = "Microsoft.VisualStudioCode"
                    Version = "1.54.3"
                    Match = "Tag: vscode"
                    Source = "winget"
                }
                {
                    Name = "Java SE Development Kit 11"
                    PackageId = "Oracle.JDK"
                    Version = "14.1"
                    Match = ""
                    Source = "winget"
                }
            ]
        @>
