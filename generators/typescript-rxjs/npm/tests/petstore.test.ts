import { setupServer } from 'msw/node';
import { toRxjsApi } from '../src';
import { toMswHandler } from './mwcMappedRestHandler';
import operations from './petstore/operations';
import { conversion as addPetConversion } from './petstore/operations/addPet';
import { conversion as findPetsConversion } from './petstore/operations/findPets';

const findPets = toMswHandler(findPetsConversion);
const addPet = toMswHandler(addPetConversion);

describe('typescript-rxjs petstore.yaml', () => {
    const wrapped = toRxjsApi(operations);
    const server = setupServer();

    beforeAll(() => server.listen());
    afterEach(() => server.resetHandlers());
    afterAll(() => server.close());

    it('can wrap a post', async () => {
        const inputPet = { name: 'Fido', tag: 'dog' };
        const outputPet = { ...inputPet, id: 1234 };
        server.use(
            addPet({ params: {}, body: inputPet, mimeType: 'application/json'}, { statusCode: 200, data: { ...outputPet }, mimeType: 'application/json' })
        );
        const response = await wrapped.addPet({ body: inputPet }).toPromise();
        expect(response.statusCode).toBe(200);
        expect(response.mimeType).toBe('application/json');
        expect(response.data).toEqual(outputPet);
    });

    it('can wrap two different posts', async () => {
        const inputPet1 = { name: 'Fido', tag: 'dog' };
        const inputPet2 = { name: 'Felix', tag: 'cat' };
        const outputPet1 = { ...inputPet1, id: 1234 };
        const outputPet2 = { ...inputPet2, id: 1235 };
        server.use(
            addPet({ params: {}, body: inputPet1, mimeType: 'application/json' }, { statusCode: 200, data: { ...outputPet1 }, mimeType: 'application/json' }),
            addPet({ params: {}, body: inputPet2, mimeType: 'application/json' }, { statusCode: 200, data: { ...outputPet2 }, mimeType: 'application/json' })
        );
        const response2 = await wrapped.addPet({ body: inputPet2 }).toPromise();
        const response1 = await wrapped.addPet({ body: inputPet1 }).toPromise();
        expect(response1.statusCode).toBe(200);
        expect(response1.mimeType).toBe('application/json');
        expect(response1.data).toEqual(outputPet1);
        expect(response2.statusCode).toBe(200);
        expect(response2.mimeType).toBe('application/json');
        expect(response2.data).toEqual(outputPet2);
    });

    it('can wrap query strings in a get', async () => {
        const inputParams = { tags: ['dog','cat'], limit: 10 };
        const outputPets = [{ name: 'Fido', tag: 'dog', id: 1234 }];
        server.use(
            findPets({ params: inputParams },
            { statusCode: 200, data: outputPets, mimeType: 'application/json' })
        );
        const response = await wrapped.findPets({ params: inputParams }).toPromise();
        expect(response.data).toEqual(outputPets);
        expect(response.statusCode).toBe(200);
        expect(response.mimeType).toBe('application/json');
    });
});