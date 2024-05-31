import { setupServer } from 'msw/node'
import fetch from 'node-fetch';
import { toMswHandler, toMswResponse } from '../src';
import operations from './petstore/operations';

const findPets = toMswHandler(operations.findPets);
const addPet = toMswHandler(operations.addPet);

describe('typescript-rxjs petstore.yaml', () => {
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
        const response = await fetch('http://localhost/pets', {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
            },
            body: JSON.stringify(inputPet),
        });
        expect(response.status).toBe(200);
        expect(response.headers.get('Content-Type')).toBe('application/json');
        expect(await response.json()).toEqual(outputPet);
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
        const response2 = await fetch('http://localhost/pets', {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
            },
            body: JSON.stringify(inputPet2),
        });
        const response1 = await fetch('http://localhost/pets', {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
            },
            body: JSON.stringify(inputPet1),
        });
        expect(response1.status).toBe(200);
        expect(response1.headers.get('Content-Type')).toBe('application/json');
        expect(await response1.json()).toEqual(outputPet1);
        expect(response2.status).toBe(200);
        expect(response2.headers.get('Content-Type')).toBe('application/json');
        expect(await response2.json()).toEqual(outputPet2);
    });

    it('can wrap any post', async () => {
        server.use(
            addPet({ params: {} }, async (req, res, ctx) => {
                return toMswResponse({
                    statusCode: 200,
                    data: { id: 1234, ...(await req.json()) },
                    mimeType: 'application/json'
                }, res, ctx);
            })
        );
        const response = await fetch('http://localhost/pets', {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
            },
            body: JSON.stringify({ name: 'Fido', tag: 'dog' }),
        });
        expect(response.status).toBe(200);
        expect(response.headers.get('Content-Type')).toBe('application/json');
        expect(await response.json()).toEqual({ name: 'Fido', tag: 'dog', id: 1234 });


        const response2 = await fetch('http://localhost/pets', {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
            },
            body: JSON.stringify({ name: 'Felix', tag: 'cat' }),
        });
        expect(await response2.json()).toEqual({ name: 'Felix', tag: 'cat', id: 1234 });
    });

    it('can wrap query strings in a get', async () => {
        const inputParams = { tags: ['dog','cat'], limit: 10 };
        const outputPets = [{ name: 'Fido', tag: 'dog', id: 1234 }];
        server.use(
            findPets(
                { params: inputParams },
                { statusCode: 200, data: outputPets, mimeType: 'application/json' }
            )
        );
        const response = await fetch('http://localhost/pets?tags=dog&tags=cat&limit=10');
        expect(await response.json()).toEqual(outputPets);
        expect(response.status).toBe(200);
        expect(response.headers.get('Content-Type')).toBe('application/json');
    });
});