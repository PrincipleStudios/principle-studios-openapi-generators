Push-Location

cd $PSScriptRoot
cd PrincipleStudios.OpenApiCodegen.Server.Mvc.MsBuild
dotnet build /p:Configuration=Release
dotnet pack --output "$PSScriptRoot/../out"

Pop-Location
