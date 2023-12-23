module finget.Tests

open DEdge.Diffract
open finget
open NUnit.Framework

[<SetUp>]
let Setup () = ()

let test input expected =

    let actual = WingetOutputParser.tryParse input

    match actual with
    | Ok results ->
        // results |> List.iter (printfn "%A")
        Differ.Assert(expected, results)

    | Error error -> Assert.Fail(error)

[<Test>]
let Test1 () =

    let input =
        """
Name                         Id                                  Version      Match          Source
---------------------------------------------------------------------------------------------------
7-Zip 19.00 (x64)            7zip.7zip                           1.9.0        Moniker: 7zip  winget
Microsoft Visual Studio Code Microsoft.VisualStudioCode          1.54.3       Tag: vscode    winget
Java SE Development Kit 11   Oracle.JDK                          14.1                        winget
"""

    let expected = [
        { Name = "7-Zip 19.00 (x64)"; PackageId = "7zip.7zip"; Version = "1.9.0"; Match = "Moniker: 7zip"; Source = "winget" }
        { Name = "Microsoft Visual Studio Code"; PackageId = "Microsoft.VisualStudioCode"; Version = "1.54.3"; Match = "Tag: vscode"; Source = "winget" }
        { Name = "Java SE Development Kit 11"; PackageId = "Oracle.JDK"; Version = "14.1"; Match = ""; Source = "winget" }
    ]

    test input expected

[<Test>]
let Test2 () =

    let input =
        """
Name                      Id                      Version      Match
-----------------------------------------------------------------------------
JetBrains ReSharper (EAP) JetBrains.ReSharper.EAP 2021.2 EAP 8 Tag: rider    
JetBrains ReSharper       JetBrains.ReSharper     2023.2.3     Tag: rider    
dotUltimate               JetBrains.dotUltimate   2023.1.4     Tag: rider    
JetBrains Rider (EAP)     JetBrains.Rider.EAP     233.9802.20                
JetBrains Rider           JetBrains.Rider         2023.3.1     Moniker: rider
"""

    let expected = [
        { Name = "JetBrains ReSharper (EAP)"; PackageId = "JetBrains.ReSharper.EAP"; Version = "2021.2 EAP 8"; Match = "Tag: rider"; Source = "" }
        { Name = "JetBrains ReSharper"; PackageId = "JetBrains.ReSharper"; Version = "2023.2.3"; Match = "Tag: rider"; Source = "" }
        { Name = "dotUltimate"; PackageId = "JetBrains.dotUltimate"; Version = "2023.1.4"; Match = "Tag: rider"; Source = "" }
        { Name = "JetBrains Rider (EAP)"; PackageId = "JetBrains.Rider.EAP"; Version = "233.9802.20"; Match = ""; Source = "" }
        { Name = "JetBrains Rider"; PackageId = "JetBrains.Rider"; Version = "2023.3.1"; Match = "Moniker: rider"; Source = "" }
    ]

    test input expected

[<Test>]
let Test3 () =

    let input =
        """
Name                           Id                           Version
--------------------------------------------------------------------------------
Microsoft .NET SDK 8.0 Preview Microsoft.DotNet.SDK.Preview 8.0.100-rc.2.23502.2
Microsoft .NET SDK 8.0         Microsoft.DotNet.SDK.8       8.0.100
Microsoft .NET SDK 7.0         Microsoft.DotNet.SDK.7       7.0.404
Microsoft .NET SDK 6.0         Microsoft.DotNet.SDK.6       6.0.417
Microsoft .NET SDK 5.0         Microsoft.DotNet.SDK.5       5.0.408
Microsoft .NET SDK 3.1         Microsoft.DotNet.SDK.3_1     3.1.426
"""

    let expected = [
        { Name = "Microsoft .NET SDK 8.0 Preview"; PackageId = "Microsoft.DotNet.SDK.Preview"; Version = "8.0.100-rc.2.23502.2"; Match = ""; Source = "" }
        { Name = "Microsoft .NET SDK 8.0"; PackageId = "Microsoft.DotNet.SDK.8"; Version = "8.0.100"; Match = ""; Source = "" }
        { Name = "Microsoft .NET SDK 7.0"; PackageId = "Microsoft.DotNet.SDK.7"; Version = "7.0.404"; Match = ""; Source = "" }
        { Name = "Microsoft .NET SDK 6.0"; PackageId = "Microsoft.DotNet.SDK.6"; Version = "6.0.417"; Match = ""; Source = "" }
        { Name = "Microsoft .NET SDK 5.0"; PackageId = "Microsoft.DotNet.SDK.5"; Version = "5.0.408"; Match = ""; Source = "" }
        { Name = "Microsoft .NET SDK 3.1"; PackageId = "Microsoft.DotNet.SDK.3_1"; Version = "3.1.426"; Match = ""; Source = "" }
    ]

    test input expected

[<Test>]
let Test4 () =

    let input =
        """

  ████████████▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒  1024 KB / 4.65 MB
  ██████████████████████████████  4.65 MB / 4.65 MB
  █████▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒  19%
  ███████████████▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒  53%
  █████████████████▒▒▒▒▒▒▒▒▒▒▒▒▒  57%
  ████████████████████▒▒▒▒▒▒▒▒▒▒  69%
  ██████████████████████▒▒▒▒▒▒▒▒  76%
  █████████████████████████▒▒▒▒▒  84%
  ██████████████████████████████  100%
Name                                                  Id                         Version        Match
---------------------------------------------------------------------------------------------------------------------
Google Chrome                                         Google.Chrome              120.0.6099.130 Moniker: chrome
Google Chrome Dev                                     Google.Chrome.Dev          121.0.6115.2   Command: chrome
Google Chrome Beta                                    Google.Chrome.Beta         120.0.6099.5   Command: chrome
Dichromate                                            Dichromate.Browser         110.0.5481.178 Command: chrome
Chrome Remote Desktop Host                            Google.ChromeRemoteDesktop 121.0.6167.13  Tag: chrome
Ginger Chrome                                         Saxo_Broko.GingerChrome    93.0.4529.0
ChromeCacheView                                       NirSoft.ChromeCacheView    2.45
ICBCChromeExtension                                   ICBC.ICBCChromeExtension   1.2.0.0
Google Chrome Canary                                  Google.Chrome.Canary       121.0.6128.2
ChromeDriver for Chrome 111                           Chromium.ChromeDriver      114.0.5735.90
360 极速浏览器X                                       360.360Chrome.X            22.1.1073.64
360极速浏览器                                         360.360Chrome              13.5.2044.0
115浏览器                                             115.115Chrome              25.0.6.5
Vision Teacher for Chromebooks Machine-Wide Installer Netop.VisionTeacher        1.7.6.0
Inssist                                               SlashedIo.Inssist          16.1.0         Tag: chrome-extension

"""

    let expected = [
        { Name = "Google Chrome"; PackageId = "Google.Chrome"; Version = "120.0.6099.130"; Match = "Moniker: chrome"; Source = "" }
        { Name = "Google Chrome Dev"; PackageId = "Google.Chrome.Dev"; Version = "121.0.6115.2"; Match = "Command: chrome"; Source = "" }
        { Name = "Google Chrome Beta"; PackageId = "Google.Chrome.Beta"; Version = "120.0.6099.5"; Match = "Command: chrome"; Source = "" }
        { Name = "Dichromate"; PackageId = "Dichromate.Browser"; Version = "110.0.5481.178"; Match = "Command: chrome"; Source = "" }
        { Name = "Chrome Remote Desktop Host"; PackageId = "Google.ChromeRemoteDesktop"; Version = "121.0.6167.13"; Match = "Tag: chrome"; Source = "" }
        { Name = "Ginger Chrome"; PackageId = "Saxo_Broko.GingerChrome"; Version = "93.0.4529.0"; Match = ""; Source = "" }
        { Name = "ChromeCacheView"; PackageId = "NirSoft.ChromeCacheView"; Version = "2.45"; Match = ""; Source = "" }
        { Name = "ICBCChromeExtension"; PackageId = "ICBC.ICBCChromeExtension"; Version = "1.2.0.0"; Match = ""; Source = "" }
        { Name = "Google Chrome Canary"; PackageId = "Google.Chrome.Canary"; Version = "121.0.6128.2"; Match = ""; Source = "" }
        { Name = "ChromeDriver for Chrome 111"; PackageId = "Chromium.ChromeDriver"; Version = "114.0.5735.90"; Match = ""; Source = "" }
        { Name = "360 ??????????X"; PackageId = "360.360Chrome.X"; Version = "22.1.1073.64"; Match = ""; Source = "" }
        { Name = "360??????????"; PackageId = "360.360Chrome"; Version = "13.5.2044.0"; Match = ""; Source = "" }
        { Name = "115??????"; PackageId = "115.115Chrome"; Version = "25.0.6.5"; Match = ""; Source = "" }
        { Name = "Vision Teacher for Chromebooks Machine-Wide Installer"; PackageId = "Netop.VisionTeacher"; Version = "1.7.6.0"; Match = ""; Source = "" }
        { Name = "Inssist"; PackageId = "SlashedIo.Inssist"; Version = "16.1.0"; Match = "Tag: chrome-extension"; Source = "" }
    ]

    test input expected
