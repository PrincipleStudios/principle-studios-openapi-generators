import rimraf from 'rimraf';
import {exec} from 'child_process';
import * as path from 'path';

const templateFolder = path.join(__dirname, "../templates");

export function runCodegen(outDir: string, args: string) {
    return new Promise<{ stdout: string; stderr: string }>((resolve, reject) => {
        rimraf(outDir, err => {
            if (err) {
                reject(err);
            }
            exec(`node node_modules/@openapitools/openapi-generator-cli/bin/openapi-generator generate ${args} -o ${outDir} -g typescript-rxjs -t "${templateFolder}"`,
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