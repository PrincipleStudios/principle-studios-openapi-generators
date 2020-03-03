import rimraf from 'rimraf';
import {exec} from 'child_process';

export function runCodegen(outDir: string, args: string) {
    return new Promise<{ stdout: string; stderr: string }>((resolve, reject) => {
        rimraf(outDir, err => {
            if (err) {
                reject(err);
            }
            exec(`node node_modules/@openapitools/openapi-generator-cli/bin/openapi-generator ${args} -o ${outDir}`,
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