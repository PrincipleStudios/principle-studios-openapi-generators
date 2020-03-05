import * as path from 'path';
import { runCodegen } from './runCodegen';
import { verifyAllFiles } from './verifyAllFiles';

describe('typescript-axios petstore.yaml', () => {
    const outputDir = path.join(__dirname, "../out.local.petstore3");
    const source = path.join(__dirname, "../../../schemas/petstore3.json");

    beforeAll(async () => {
        await runCodegen(outputDir, `-i "${source}"`);
    })

    verifyAllFiles(outputDir);
});