import * as path from 'path';
import { readFileSync } from 'fs';
import { runCodegen } from './runCodegen';

const templateFolder = path.join(__dirname, "../templates")
const outputDir = path.join(__dirname, "../out.local")
const source = path.join(__dirname, "../../samples/petstore.yaml");

describe('typescript-client-fetch', () => {
    beforeAll(async () => {
        await runCodegen(outputDir, `generate -i "${source}" -g typescript-axios -t "${templateFolder}"`);
    })

    it('can generate the api file', async () => {
        // load a config and a definition
        const contents = readFileSync(path.join(outputDir, "api.ts")).toString();

        expect(contents).toMatchSnapshot();
    });

    it('can generate the base file', async () => {
        // load a config and a definition
        const contents = readFileSync(path.join(outputDir, "base.ts")).toString();

        expect(contents).toMatchSnapshot();
    });

    it('can generate the configuration file', async () => {
        // load a config and a definition
        const contents = readFileSync(path.join(outputDir, "configuration.ts")).toString();

        expect(contents).toMatchSnapshot();
    });

    it('can generate the index file', async () => {
        // load a config and a definition
        const contents = readFileSync(path.join(outputDir, "index.ts")).toString();

        expect(contents).toMatchSnapshot();
    });
});