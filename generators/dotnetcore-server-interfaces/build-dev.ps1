Push-Location

$date = (Get-Date).ToString('yyyyMMddTHHmmss')

cd $PSScriptRoot
cd PrincipleStudios.OpenApiCodegen.Server.Mvc
dotnet build /p:Configuration=Debug /p:VersionSuffix=date.$date
dotnet pack /p:VersionSuffix=date.$date --output "$PSScriptRoot/../out"

Pop-Location
