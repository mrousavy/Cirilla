#!/bin/bash
cd "$(dirname "$0")"
dotnet build -c Release
dotnet publish -c Release