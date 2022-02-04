import { toRxjsApi } from '../src';
import operations from './petstore/operations';
import type {Responses} from './petstore/operations/addPet';
import type { Observable } from 'rxjs';

describe('typescript-rxjs petstore.yaml', () => {
    it('can be wrapped with rxjs', () => {
        const wrapped = toRxjsApi(operations);
        const petResponse: Observable<Responses> = wrapped.addPet({}, { name: 'Fido', tag: 'dog' }, 'application/json');
    })
});