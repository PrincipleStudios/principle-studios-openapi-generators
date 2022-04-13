Push-Location

cd $PSScriptRoot
cd PrincipleStudios.OpenApiCodegen.Client
dotnet build /p:Configuration=Debug
dotnet pack --output "$PSScriptRoot/../out"

Pop-Location
