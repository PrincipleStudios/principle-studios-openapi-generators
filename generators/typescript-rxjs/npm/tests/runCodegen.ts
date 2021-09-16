import {spawn} from 'child_process';

export function runCodegen(input: string, outDir: string, ...args: string[]) {
    return new Promise<{}>((resolve, reject) => {
        spawn(`dotnet`, [`node_modules/@principlestudios/openapi-codegen-typescript/dotnet/PrincipleStudios.OpenApiCodegen.Client.TypeScript.dll`, input, outDir, ...args], { stdio: 'inherit' }).on('close', code => {
            if (code !== 0) {
                reject({ code });
            } else {
                resolve({});
            }
        });
    });
}