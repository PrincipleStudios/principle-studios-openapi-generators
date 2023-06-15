import { toRxjsApi } from '../src';
import operations from './petstore/operations';
import type {Responses as FindPetsResponses} from './petstore/operations/findPets';
import type {Responses as AddPetResponses} from './petstore/operations/addPet';
import type { Observable } from 'rxjs';
import { rest } from 'msw'
import { setupServer } from 'msw/node'

global.XMLHttpRequest = require("xmlhttprequest").XMLHttpRequest;

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
            rest.post('http://localhost/pets', async (req, res, ctx) => {
                try {
                    expect(await req.json()).toEqual({ ...inputPet });
                } catch (ex) {
                    return res(ctx.status(500), ctx.json(ex as any));
                }
                return res(
                    ctx.status(200),
                    ctx.json({ ...outputPet }),
                    ctx.set('Content-Type', 'application/json')
                );
            }))
        const response = await wrapped.addPet({ body: inputPet }).toPromise();
        expect(response.statusCode).toBe(200);
        expect(response.mimeType).toBe('application/json');
        expect(response.data).toEqual(outputPet);
    });

    it('can wrap query strings in a get', async () => {
        const outputPets = [{ name: 'Fido', tag: 'dog', id: 1234 }];
        server.use(
            rest.get('http://localhost/pets', async (req, res, ctx) => {
                try {
                    expect(req.url.searchParams.getAll('tags')).toEqual(['dog', 'cat']);
                    expect(req.url.searchParams.get('limit')).toBe('10');
                } catch (ex) {
                    return res(ctx.status(500), ctx.json(ex as any));
                }
                return res(
                    ctx.status(200),
                    ctx.json([...outputPets]),
                    ctx.set('Content-Type', 'application/json')
                );
            }))
        const response = await wrapped.findPets({ params: { tags: ['dog', 'cat'], limit: 10 } }).toPromise();
        expect(response.data).toEqual(outputPets);
        expect(response.statusCode).toBe(200);
        expect(response.mimeType).toBe('application/json');
    });
});