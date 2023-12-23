namespace finget

type InstalledPackage = {
    Name: string
    PackageId: string
    Version: string
    Available: string
    Source: string
} with

    static member init f = {
        Name = f "Name"
        PackageId = f "Id"
        Version = f "Version"
        Available = f "Available"
        Source = f "Source"
    }

type SearchResult = {
    Name: string
    PackageId: string
    Version: string
    Match: string
    Source: string
} with

    static member init f = {
        Name = f "Name"
        PackageId = f "Id"
        Version = f "Version"
        Match = f "Match"
        Source = f "Source"
    }

    member this.asInstalledPackage =
        {
            Name = this.Name
            PackageId = this.PackageId
            Version = this.Version
            Available = ""
            Source = this.Source
        }
