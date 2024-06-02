/* eslint-disable @typescript-eslint/ban-types */
/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable @typescript-eslint/naming-convention */
/* eslint-disable @typescript-eslint/no-unsafe-argument */
// TODO: this likely needs to be rewritten to support newer msw anyway
import type {
	AdapterRequestArgs,
	HttpMethod,
	RequestBodies,
	RequestConversion,
	StandardResponse,
	TransformCallType,
} from '@principlestudios/openapi-codegen-typescript';
import type {
	HttpHandler,
	DefaultRequestMultipartBody,
	JsonBodyType,
	StrictResponse,
	ResponseResolverReturnType,
} from 'msw';
import { http, HttpResponse } from 'msw';
import type { ResponseResolverInfo } from 'msw/lib/core/handlers/RequestHandler';

function deepEqual(x: unknown, y: unknown): boolean {
	const tx = typeof x,
		ty = typeof y;
	if (!x || !y || tx !== 'object' || tx !== ty) return x === y;

	const xKeys = Object.keys(x),
		yKeys = Object.keys(y);
	return (
		xKeys.length === yKeys.length &&
		xKeys.every((key) =>
			deepEqual(
				(x as Record<string, unknown>)[key],
				(y as Record<string, unknown>)[key],
			),
		)
	);
}

type DefaultBodyType = DefaultRequestMultipartBody | JsonBodyType;
type SafeStandardResponse = StandardResponse<
	number | 'other',
	string,
	DefaultBodyType
>;
type SafeResponseResolverReturnType<TResponses extends SafeStandardResponse> =
	ResponseResolverReturnType<TResponses['data']>;

export type AsyncResponseResolver<TResponses extends SafeStandardResponse> = (
	info: ResponseResolverInfo<Record<string, unknown>>,
) =>
	| SafeResponseResolverReturnType<TResponses>
	| Promise<SafeResponseResolverReturnType<TResponses>>;

export type MswStandardResponse<
	T extends SafeStandardResponse = SafeStandardResponse,
> = {
	statusCode: T['statusCode'];
	mimeType: T['mimeType'];
	data: T['data'];
	headers?: Record<string, unknown>;
};

export function toMswResponse<
	TStatusCode extends number | 'other' = number | 'other',
	TMimeType extends string = string,
	TBody extends DefaultBodyType = undefined,
>(
	response: MswStandardResponse<
		StandardResponse<TStatusCode, TMimeType, TBody>
	>,
): StrictResponse<TBody> {
	return HttpResponse.json<TBody>(response.data, {
		status: response.statusCode === 'other' ? 0 : response.statusCode,
		// TODO: support headers
	});
}

function buildHandler<
	TBody extends DefaultBodyType,
	TResponses extends StandardResponse<number | 'other', string, TBody>,
>(
	request: AdapterRequestArgs,
	response: AsyncResponseResolver<TResponses> | MswStandardResponse<TResponses>,
	options?: RequestHandlerOptions,
) {
	const methodFunc =
		http[request.method.toLowerCase() as Lowercase<HttpMethod>];
	const url = new URL(
		request.path.split('?')[0],
		options?.baseDomain,
	).toString();
	return methodFunc<
		any,
		any,
		TResponses['data'] extends DefaultBodyType
			? TResponses['data']
			: DefaultBodyType
	>(url, (info) => handleMswFullRequest(info, request, response), options);
}

async function handleMswFullRequest<
	TBody extends DefaultBodyType,
	TResponses extends StandardResponse<number | 'other', string, TBody>,
>(
	info: ResponseResolverInfo<Record<string, unknown>>,
	request: AdapterRequestArgs,
	response: AsyncResponseResolver<TResponses> | MswStandardResponse<TResponses>,
): Promise<SafeResponseResolverReturnType<TResponses>> {
	if (!queryStringMatch(request, info.request)) {
		return;
	}
	if ('body' in request) {
		const currentBody = await info.request.clone().json();
		if (!deepEqual(request.body, currentBody)) {
			return;
		}
	} else if (info.request.bodyUsed) {
		return;
	}
	// TODO: request headers aren't checked

	if (typeof response === 'function') return response(info);
	return toMswResponse(response);
}

function queryStringMatch(thisRequest: AdapterRequestArgs, request: Request) {
	return (
		new URL(request.url).searchParams.toString() ==
		(thisRequest.path.split('?')[1] ?? '')
	);
}

type RequestHandlerOptions = {
	once?: boolean;
	baseDomain?: string;
};

type MatchedRequest<
	TRequestParams extends {} = {},
	TRequestBodies extends RequestBodies = RequestBodies,
	TCallType extends TransformCallType = TransformCallType,
> =
	| (TCallType extends 'no-body' | 'optional'
			? { params: TRequestParams; body?: never; mimeType?: never }
			: never)
	| (TCallType extends 'body' | 'optional'
			? {
					[K in keyof TRequestBodies]: {
						params: TRequestParams;
						body?: TRequestBodies[K];
						mimeType?: K;
					};
				}[keyof TRequestBodies]
			: never);

export function toMswHandler<
	TMethod extends HttpMethod,
	TUrlParams extends {},
	TRequestParams extends TUrlParams,
	TRequestBodies extends RequestBodies,
	TBody extends DefaultBodyType,
	TResponses extends StandardResponse<number | 'other', string, TBody>,
	TCallType extends TransformCallType,
>(
	conversion: RequestConversion<
		TMethod,
		TUrlParams,
		TRequestParams,
		TRequestBodies,
		TResponses,
		TCallType
	>,
	defaultOptions?: RequestHandlerOptions,
) {
	type MyRequest = MatchedRequest<TRequestParams, TRequestBodies, TCallType>;

	return function (
		request: MyRequest,
		response:
			| AsyncResponseResolver<TResponses>
			| MswStandardResponse<TResponses>,
		options?: RequestHandlerOptions,
	): HttpHandler {
		const standardRequest: AdapterRequestArgs =
			'body' in request
				? conversion.request(
						request.params,
						request.body as TRequestBodies[keyof TRequestBodies],
						request.mimeType as keyof TRequestBodies,
					)
				: {
						method: conversion.method,
						path: conversion.url(request.params),
						// TODO: headers support?
					};
		return buildHandler(standardRequest, response, {
			...defaultOptions,
			...options,
		});
	};
}
