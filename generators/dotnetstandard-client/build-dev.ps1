Push-Location

cd $PSScriptRoot
cd PrincipleStudios.OpenApiCodegen.Client.MsBuild
dotnet build /p:Configuration=Debug
dotnet pack --output "$PSScriptRoot/../out"

Pop-Location
