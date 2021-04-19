Push-Location

cd $PSScriptRoot
cd PrincipleStudios.OpenApiCodegen.Server.Mvc.Executable
dotnet build /p:Configuration=Release
dotnet pack --output "$PSScriptRoot/../out"

Pop-Location
