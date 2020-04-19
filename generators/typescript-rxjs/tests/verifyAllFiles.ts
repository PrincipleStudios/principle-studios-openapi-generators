import * as path from 'path';
import { readFileSync, writeFileSync, readdirSync } from 'fs';
import { CLIEngine } from "eslint";

export function verifyAllFiles(outputDir: string) {
    it('can generate the files', () => {
        const files = readdirSync(outputDir);

        files.forEach((file) => {
            if (file.endsWith('.cs')) {
                const contents = readFileSync(path.join(outputDir, file)).toString();
                expect(contents).toMatchSnapshot(file);
            }
        });
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
            writeFileSync(outputDir + '/lint-results.json', JSON.stringify(report.results, undefined, '  '));
            console.error(`See ${outputDir + '/lint-results.json'} for more info`);
        }
        expect(report.errorCount).toEqual(0);
    });
}
