Push-Location

cd $PSScriptRoot
cd PrincipleStudios.OpenApiCodegen.Server.Mvc.Executable
dotnet build /p:Configuration=Debug
dotnet pack --output "$PSScriptRoot/../out"

Pop-Location
