Push-Location

cd $PSScriptRoot
dotnet publish ./PrincipleStudios.OpenApiCodegen.Client.TypeScript/PrincipleStudios.OpenApiCodegen.Client.TypeScript.csproj -c Release -p:UseAppHost=false -o ./npm/dotnet

cd npm
Remove-Item -LiteralPath "lib" -Force -Recurse -ErrorAction SilentlyContinue
npm pack

cd $PSScriptRoot
mkdir ../out -Force
remove-item ../out/principlestudios-openapi-codegen-typescript-[0-9]*.tgz
move-item ./npm/principlestudios-openapi-codegen-typescript-[0-9]*.tgz ../out

Pop-Location
