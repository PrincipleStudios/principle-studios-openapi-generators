import * as path from 'path';
import { runCodegen } from './runCodegen';
import { verifyAllFiles } from './verifyAllFiles';

describe('typescript-axios petstore.yaml', () => {
    const templateFolder = path.join(__dirname, "../templates");
    const outputDir = path.join(__dirname, "../out.local.petstore3");
    const source = path.join(__dirname, "../../samples/petstore3.json");

    beforeAll(async () => {
        await runCodegen(outputDir, `generate -i "${source}" -g typescript-axios -t "${templateFolder}"`);
    })

    verifyAllFiles(outputDir);
});