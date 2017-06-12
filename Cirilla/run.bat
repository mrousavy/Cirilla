SET mypath=%~dp0
dotnet build -f netcoreapp2.0 -c Release
dotnet run --framework netcoreapp2.0