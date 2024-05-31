import { toMswHandler } from '@principlestudios/openapi-codegen-typescript-msw';
import { setupServer } from 'msw/node'
import fetch from 'node-fetch';
import { toFetchApi, toFetchOperation } from '../src';
import type { FetchImplementation} from '../src';
import operations from './multi-path-variables/operations';

const fetchImpl: FetchImplementation<unknown>  = (url, params) => fetch('http://localhost' + String(url), params);
const fetchApi = toFetchApi(operations, fetchImpl);
const getPhotoMeta = toMswHandler(operations.getPhotoMeta);

describe('typescript-fetch multi-path-variables.yaml', () => {
    const server = setupServer();

    beforeAll(() => server.listen());
    afterEach(() => server.resetHandlers());
    afterAll(() => server.close());

    it('can wrap a get as a single operation', async () => {
        const getPhotoMetaOperation = toFetchOperation(fetchImpl, operations.getPhotoMeta);
        const outputMeta = 'unknown data';
        server.use(
            getPhotoMeta({ params: { id: '123', key: 'author'} }, { statusCode: 200, data: outputMeta, mimeType: 'application/json' })
        );
        const response = await getPhotoMetaOperation({ params: { id: '123', key: 'author'} });
        expect(response.statusCode).toBe(200);
        expect(response.response.getResponseHeader('Content-Type')).toBe('application/json');
        expect(response.data).toEqual(outputMeta);
    });

    it('can wrap a get', async () => {
        const outputMeta = 'unknown data';
        server.use(
            getPhotoMeta({ params: { id: '123', key: 'author'} }, { statusCode: 200, data: outputMeta, mimeType: 'application/json' })
        );
        const response = await fetchApi.getPhotoMeta({ params: { id: '123', key: 'author'} });
        expect(response.statusCode).toBe(200);
        expect(response.response.getResponseHeader('Content-Type')).toBe('application/json');
        expect(response.data).toEqual(outputMeta);
    });
});
