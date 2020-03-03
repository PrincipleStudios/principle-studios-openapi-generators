import * as path from 'path';
import { readFileSync, readdirSync } from 'fs';

export function verifyAllFiles(outputDir: string) {
    it('can generate the files', () => {
        const files = readdirSync(outputDir);

        files.forEach((file) => {
            if (file.endsWith('.cs')) {
                const contents = readFileSync(path.join(outputDir, file)).toString();
                expect(contents).toMatchSnapshot();
            }
        });
    });
}
