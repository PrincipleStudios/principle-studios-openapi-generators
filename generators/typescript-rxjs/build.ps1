Push-Location

cd $PSScriptRoot/npm
remove-item ./principlestudios-openapi-codegen-typescript-rxjs-[0-9]*.tgz
npm pack
mkdir ../../out -Force
remove-item ../../out/principlestudios-openapi-codegen-typescript-rxjs-[0-9]*.tgz
move-item ./principlestudios-openapi-codegen-typescript-rxjs-[0-9]*.tgz ../../out

Pop-Location
