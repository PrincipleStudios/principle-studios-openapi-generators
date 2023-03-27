#!/usr/bin/env pwsh

Param(
    [Parameter(Mandatory)][String] $VersionSuffix,
    [Parameter()][String] $githubToken
)

Push-Location "$PSScriptRoot/../../.."
try {
    docker build . -f examples/dotnetcore-server-interfaces/tooling/Dockerfile `
        --build-arg VersionSuffix="$VersionSuffix" `
        --build-arg GitHubToken="$githubToken"
    if ($Global:LASTEXITCODE -ne 0) {
        throw "Docker build failed with exit code $Global:LASTEXITCODE"
    }
} finally {
    Pop-Location
}