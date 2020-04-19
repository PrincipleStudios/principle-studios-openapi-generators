Push-Location

cd $PSScriptRoot
cd nuget
dotnet pack --output "$PSScriptRoot/../out"

Pop-Location
