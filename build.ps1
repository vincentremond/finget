$ErrorActionPreference = "Stop"

dotnet tool restore
dotnet build

AddToPath .\finget\bin\Debug\
