Push-Location

cd $PSScriptRoot
cd PrincipleStudios.OpenApiCodegen.Client
dotnet build /p:Configuration=Release
dotnet pack /p:Configuration=Release --output "$PSScriptRoot/../out"

Pop-Location
