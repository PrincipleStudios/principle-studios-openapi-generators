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
} finally {
    Pop-Location
}