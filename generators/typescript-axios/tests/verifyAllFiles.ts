import * as path from 'path';
import { readFileSync } from 'fs';
import { CLIEngine } from "eslint";

export function verifyAllFiles(outputDir: string) {
    it('can generate the api file', () => {
        const contents = readFileSync(path.join(outputDir, "api.ts")).toString();
        expect(contents).toMatchSnapshot();
    });

    it('can generate the base file', () => {
        const contents = readFileSync(path.join(outputDir, "base.ts")).toString();
        expect(contents).toMatchSnapshot();
    });

    it('can generate the configuration file', () => {
        const contents = readFileSync(path.join(outputDir, "configuration.ts")).toString();
        expect(contents).toMatchSnapshot();
    });

    it('can generate the index file', () => {
        const contents = readFileSync(path.join(outputDir, "index.ts")).toString();
        expect(contents).toMatchSnapshot();
    });

    it('ignores weird lint settings', () => {
        const cli = new CLIEngine({
            baseConfig: { extends: ["plugin:@typescript-eslint/recommended"] },
            rules: {
                semi: ["error", "never"]
            }
        });
        const report = cli.executeOnFiles([`${outputDir}/**/*.ts`]);
        if (report.errorCount > 0) {
            console.error(JSON.stringify(report.results));
        }
        expect(report.errorCount).toEqual(0);
    });
}
