@ECHO OFF

dotnet tool restore
dotnet build -- %*

AddToPath .\finget\bin\Debug\
