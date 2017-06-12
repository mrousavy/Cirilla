SET mypath=%~dp0
dotnet build -f netcoreapp1.1 -c Release
dotnet publish -f netcoreapp1.1 -c Release