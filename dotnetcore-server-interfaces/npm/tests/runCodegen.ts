import rimraf from 'rimraf';
import {exec} from 'child_process';
import * as path from 'path';
import * as fs from 'fs';

export function runCodegen(outDir: string, args: string) {
    return new Promise<{ stdout: string; stderr: string }>((resolve, reject) => {
        rimraf(outDir, err => {
            if (err) {
                reject(err);
            }

            const myJar = path.join(__dirname, "../../java/build/libs/aspnetcore-server-interfaces-1.0.0.jar");
            const origJar = require.resolve('@openapitools/openapi-generator-cli/bin/openapi-generator.jar');
            if (!fs.existsSync(myJar)) throw new Error("My jar doesn't exist!");
            if (!fs.existsSync(origJar)) throw new Error("The original jar doesn't exist!");

            exec(`java -cp "${myJar};${origJar}" org.openapitools.codegen.OpenAPIGenerator ${args}  -g com.principlestudios.codegen.DotNetCoreInterfacesGenerator -o ${outDir}`,
                (error, stdout, stderr) => {
                    if (error !== null) {
                        reject({ stdout, stderr, error });
                    } else {
                        resolve({ stdout, stderr });
                    }
            });
        });
    });
}