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

            const myJars = path.join(__dirname, "../../java/build/libs");
            if (!fs.existsSync(myJars)) throw new Error("My jar doesn't exist!");

            exec(`java -cp "${myJars}/*" org.openapitools.codegen.OpenAPIGenerator ${args} -g com.principlestudios.codegen.DotNetStandardClientGenerator -o ${outDir}`,
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