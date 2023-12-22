module finget.Tests

open DEdge.Diffract
open finget
open NUnit.Framework

[<SetUp>]
let Setup () = ()

[<Test>]
let Test1 () =

    let actual =
        WingetOutputParser.tryParse
            """
Name                         Id                                  Version      Match          Source
---------------------------------------------------------------------------------------------------
7-Zip 19.00 (x64)            7zip.7zip                           1.9.0        Moniker: 7zip  winget
Microsoft Visual Studio Code Microsoft.VisualStudioCode          1.54.3       Tag: vscode    winget
Java SE Development Kit 11   Oracle.JDK                          14.1                        winget
"""

    let expected =
        Ok [
            {
                Name = "7-Zip 19.00 (x64)"
                PackageId = "7zip.7zip"
                Version = "1.9.0"
                Match = "Moniker: 7zip"
                Source = Some "winget"
            }
            {
                Name = "Microsoft Visual Studio Code"
                PackageId = "Microsoft.VisualStudioCode"
                Version = "1.54.3"
                Match = "Tag: vscode"
                Source = Some "winget"
            }
            {
                Name = "Java SE Development Kit 11"
                PackageId = "Oracle.JDK"
                Version = "14.1"
                Match = ""
                Source = Some "winget"
            }
        ]

    Differ.Assert(expected, actual)

[<Test>]
let Test2 () =

    let actual =
        WingetOutputParser.tryParse
            """
Name                      Id                      Version      Match
-----------------------------------------------------------------------------
JetBrains ReSharper (EAP) JetBrains.ReSharper.EAP 2021.2 EAP 8 Tag: rider    
JetBrains ReSharper       JetBrains.ReSharper     2023.2.3     Tag: rider    
dotUltimate               JetBrains.dotUltimate   2023.1.4     Tag: rider    
JetBrains Rider (EAP)     JetBrains.Rider.EAP     233.9802.20                
JetBrains Rider           JetBrains.Rider         2023.3.1     Moniker: rider
"""

    let expected =
        Ok [
            {
                Name = "JetBrains ReSharper (EAP)"
                PackageId = "JetBrains.ReSharper.EAP"
                Version = "2021.2 EAP 8"
                Match = "Tag: rider"
                Source = None
            }
            {
                Name = "JetBrains ReSharper"
                PackageId = "JetBrains.ReSharper"
                Version = "2023.2.3"
                Match = "Tag: rider"
                Source = None
            }
            {
                Name = "dotUltimate"
                PackageId = "JetBrains.dotUltimate"
                Version = "2023.1.4"
                Match = "Tag: rider"
                Source = None
            }
            {
                Name = "JetBrains Rider (EAP)"
                PackageId = "JetBrains.Rider.EAP"
                Version = "233.9802.20"
                Match = ""
                Source = None
            }
            {
                Name = "JetBrains Rider"
                PackageId = "JetBrains.Rider"
                Version = "2023.3.1"
                Match = "Moniker: rider"
                Source = None
            }
        ]

    printfn $"%A{actual}"
    
    Differ.Assert(expected, actual)
