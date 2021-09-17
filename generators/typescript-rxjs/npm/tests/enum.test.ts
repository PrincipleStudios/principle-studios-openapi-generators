import * as path from 'path';
import { runCodegen } from './runCodegen';
import { verifyAllFiles } from './verifyAllFiles';

describe('typescript-rxjs enum.yaml', () => {
    const outputDir = path.join(__dirname, "../out.local.petstore3");
    const source = path.join(__dirname, "../../../../schemas/enum.yaml");

    beforeAll(async () => {
        await runCodegen(source, outputDir, `-c`);
    })

    verifyAllFiles(outputDir);
});