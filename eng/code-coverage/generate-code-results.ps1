#!/usr/bin/env pwsh

Push-Location "$PSScriptRoot/../.."
try {

    # Remove old test results and code coverage report. `dotnet test` adds an extra guid underneath the results-directory that is impossible to ascertain, so a wildcard is needed.
    Remove-Item -r artifacts/TestResults,artifacts/coveragereport -ErrorAction SilentlyContinue

    # Generates code coverage in the xml "cobertura" format and then generates html report files. They can be seen at `artifacts/coveragereport/index.html` in your web browser.
    dotnet test --collect:"XPlat Code Coverage" --results-directory:artifacts/TestResults --filter 'Category!=Integration'
    reportgenerator -reports:"artifacts/TestResults/*/coverage.cobertura.xml" -targetdir:artifacts/coveragereport -reporttypes:"Html;HtmlSummary"

} finally {
    Pop-Location
}