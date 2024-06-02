import {
	toMswHandler,
	toMswResponse,
} from '@principlestudios/openapi-codegen-typescript-msw';
import { setupServer } from 'msw/node';
import fetch from 'node-fetch';
import { describe, beforeAll, afterEach, afterAll, it, expect } from 'vitest';
import { toFetchApi, toFetchOperation } from '../src';
import type { FetchImplementation } from '../src';
import type { NewPet } from './petstore/models';
import operations from './petstore/operations';

const baseDomain = 'http://localhost/';
const fetchImpl: FetchImplementation<unknown> = (url, params) => {
	return fetch(new URL(url, baseDomain), params);
};
const fetchApi = toFetchApi(operations, fetchImpl);
const findPets = toMswHandler(operations.findPets, { baseDomain });
const addPet = toMswHandler(operations.addPet, { baseDomain });

describe('typescript-fetch petstore.yaml', () => {
	const server = setupServer();

	beforeAll(() => server.listen());
	afterEach(() => server.resetHandlers());
	afterAll(() => server.close());

	it('can wrap a post as a single operation', async () => {
		const addPetOperation = toFetchOperation(fetchImpl, operations.addPet);
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
		const response = await addPetOperation({ body: inputPet });
		expect(response.statusCode).toBe(200);
		expect(response.response.getResponseHeader('Content-Type')).toBe(
			'application/json',
		);
		expect(response.data).toEqual(outputPet);
	});

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
		const response = await fetchApi.addPet({ body: inputPet });
		expect(response.statusCode).toBe(200);
		expect(response.response.getResponseHeader('Content-Type')).toBe(
			'application/json',
		);
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
		const response2 = await fetchApi.addPet({ body: inputPet2 });
		const response1 = await fetchApi.addPet({ body: inputPet1 });
		expect(response1.statusCode).toBe(200);
		expect(response1.response.getResponseHeader('Content-Type')).toBe(
			'application/json',
		);
		expect(response1.data).toEqual(outputPet1);
		expect(response2.statusCode).toBe(200);
		expect(response2.response.getResponseHeader('Content-Type')).toBe(
			'application/json',
		);
		expect(response2.data).toEqual(outputPet2);
	});

	it('can wrap any post', async () => {
		server.use(
			addPet({ params: {} }, async (info) => {
				return toMswResponse({
					statusCode: 200,
					data: { id: 1234, ...((await info.request.json()) as NewPet) },
					mimeType: 'application/json',
				});
			}),
		);
		const response = await fetchApi.addPet({
			body: { name: 'Fido', tag: 'dog' },
		});
		expect(response.statusCode).toBe(200);
		expect(response.response.getResponseHeader('Content-Type')).toBe(
			'application/json',
		);
		expect(response.data).toEqual({ name: 'Fido', tag: 'dog', id: 1234 });

		const response2 = await fetch('http://localhost/pets', {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
			body: JSON.stringify({ name: 'Felix', tag: 'cat' }),
		});
		expect(await response2.json()).toEqual({
			name: 'Felix',
			tag: 'cat',
			id: 1234,
		});
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
		const response = await fetchApi.findPets({ params: inputParams });
		expect(response.data).toEqual(outputPets);
		expect(response.statusCode).toBe(200);
		expect(response.response.getResponseHeader('Content-Type')).toBe(
			'application/json',
		);
	});
});
