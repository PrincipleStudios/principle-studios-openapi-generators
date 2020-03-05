cd $PSScriptRoot
remove-item ./principlestudios-openapi-codegen-typescript-axios-*.tgz
npm pack
mkdir ../out -Force
remove-item ../out/principlestudios-openapi-codegen-typescript-axios-*.tgz
move-item ./principlestudios-openapi-codegen-typescript-axios-*.tgz ../out
