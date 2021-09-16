import * as path from 'path';
import { toRxjsApi } from '../src';
import { runCodegen } from './runCodegen';
import { verifyAllFiles } from './verifyAllFiles';
import operations from './petstore/operations';
import type {Responses} from './petstore/operations/addPet';
import type { Observable } from 'rxjs';

describe('typescript-rxjs petstore.yaml', () => {
    const outputDir = path.join(__dirname, "../out.local.petstore");
    const source = path.join(__dirname, "../../../../schemas/petstore.yaml");

    beforeAll(async () => {
        await runCodegen(source, outputDir, `-c`);
    })

    verifyAllFiles(outputDir);

    it('can be wrapped with rxjs', () => {
        const wrapped = toRxjsApi(operations);
        const petResponse: Observable<Responses> = wrapped.addPet({}, { name: 'Fido', tag: 'dog' }, 'application/json');
    })
});