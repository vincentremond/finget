@ECHO OFF

dotnet tool restore
dotnet build -- %*

add-to-path .\finget\bin\Debug\
