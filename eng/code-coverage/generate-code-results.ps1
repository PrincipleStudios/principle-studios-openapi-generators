#!/usr/bin/env pwsh

Push-Location "$PSScriptRoot/../.."
try {

    Remove-Item -r artifacts/TestResults,artifacts/coveragereport -ErrorAction SilentlyContinue
    dotnet test --collect:"XPlat Code Coverage" --results-directory:artifacts/TestResults --filter 'Category!=Integration&Category!=Snapshot'

    reportgenerator -reports:"artifacts/TestResults/*/coverage.cobertura.xml" -targetdir:artifacts/coveragereport -reporttypes:"Html;HtmlSummary"

} finally {
    Pop-Location
}