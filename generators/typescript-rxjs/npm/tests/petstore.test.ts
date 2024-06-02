import { toMswHandler } from '@principlestudios/openapi-codegen-typescript-msw';
import { setupServer } from 'msw/node';
import { lastValueFrom } from 'rxjs';
import { request as ajax } from 'universal-rxjs-ajax';
import { describe, beforeAll, afterEach, afterAll, it, expect } from 'vitest';
import { toRxjsApi } from '../src';
import operations from './petstore/operations';
import { conversion as addPetConversion } from './petstore/operations/addPet';
import { conversion as findPetsConversion } from './petstore/operations/findPets';

const baseDomain = 'http://localhost/';
const findPets = toMswHandler(findPetsConversion, { baseDomain });
const addPet = toMswHandler(addPetConversion, { baseDomain });

describe('typescript-rxjs petstore.yaml', () => {
	const wrapped = toRxjsApi(operations, baseDomain, ajax);
	const server = setupServer();

	beforeAll(() => server.listen());
	afterEach(() => server.resetHandlers());
	afterAll(() => server.close());

	it('can wrap a post', async () => {
		const inputPet = { name: 'Fido', tag: 'dog' };
		const outputPet = { ...inputPet, id: 1234 };
		server.use(
			addPet(
				{ params: {}, body: inputPet, mimeType: 'application/json' },
				{
					statusCode: 200,
					data: { ...outputPet },
					mimeType: 'application/json',
				},
			),
		);
		const response = await lastValueFrom(wrapped.addPet({ body: inputPet }));
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
			addPet(
				{ params: {}, body: inputPet1, mimeType: 'application/json' },
				{
					statusCode: 200,
					data: { ...outputPet1 },
					mimeType: 'application/json',
				},
			),
			addPet(
				{ params: {}, body: inputPet2, mimeType: 'application/json' },
				{
					statusCode: 200,
					data: { ...outputPet2 },
					mimeType: 'application/json',
				},
			),
		);
		const response2 = await lastValueFrom(wrapped.addPet({ body: inputPet2 }));
		const response1 = await lastValueFrom(wrapped.addPet({ body: inputPet1 }));
		expect(response1.statusCode).toBe(200);
		expect(response1.mimeType).toBe('application/json');
		expect(response1.data).toEqual(outputPet1);
		expect(response2.statusCode).toBe(200);
		expect(response2.mimeType).toBe('application/json');
		expect(response2.data).toEqual(outputPet2);
	});

	it('can wrap query strings in a get', async () => {
		const inputParams = { tags: ['dog', 'cat'], limit: 10 };
		const outputPets = [{ name: 'Fido', tag: 'dog', id: 1234 }];
		server.use(
			findPets(
				{ params: inputParams },
				{ statusCode: 200, data: outputPets, mimeType: 'application/json' },
			),
		);
		const response = await lastValueFrom(
			wrapped.findPets({ params: inputParams }),
		);
		expect(response.data).toEqual(outputPets);
		expect(response.statusCode).toBe(200);
		expect(response.mimeType).toBe('application/json');
	});
});
