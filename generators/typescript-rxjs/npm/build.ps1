cd $PSScriptRoot
remove-item ./principlestudios-openapi-codegen-typescript-rxjs-*.tgz
npm pack
mkdir ../out -Force
remove-item ../out/principlestudios-openapi-codegen-typescript-rxjs-*.tgz
move-item ./principlestudios-openapi-codegen-typescript-rxjs-*.tgz ../out
