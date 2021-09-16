#!/usr/bin/env pwsh

Push-Location "$PSScriptRoot"
try {

    dotnet publish ../PrincipleStudios.OpenApiCodegen.Client.TypeScript/PrincipleStudios.OpenApiCodegen.Client.TypeScript.csproj -c Release -p:UseAppHost=false -o ../npm/dotnet
    Remove-Item -LiteralPath "lib" -Force -Recurse -ErrorAction SilentlyContinue
    npm run tsc

} finally {
    Pop-Location
}
