Push-Location

cd $PSScriptRoot
cd PrincipleStudios.OpenApiCodegen.Server.Mvc
dotnet build
dotnet pack --output "$PSScriptRoot/../out"

Pop-Location
