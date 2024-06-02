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
	DefaultBodyType,
	Match,
	MockedRequest,
	MockedResponse,
	ResponseResolverReturnType,
	ResponseComposition,
	RestContext,
} from 'msw';
import { RestHandler } from 'msw';

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

export type AsyncResponseResolver<
	RequestType = MockedRequest,
	BodyType extends DefaultBodyType = any,
> = (
	req: RequestType,
	res: ResponseComposition<BodyType>,
	context: RestContext,
) =>
	| ResponseResolverReturnType<MockedResponse<BodyType>>
	| Promise<ResponseResolverReturnType<MockedResponse<BodyType>>>;

export type SafeResponse<T extends StandardResponse = StandardResponse> = Omit<
	T,
	'response'
> & { headers?: Record<string, unknown> };

export function toMswResponse(
	response: SafeResponse,
	res: ResponseComposition,
	ctx: RestContext,
) {
	return res(
		ctx.status(response.statusCode === 'other' ? 0 : response.statusCode),
		ctx.json(response.data),
		// TODO: support headers
	);
}

export class MappedRestHandler<
	T extends MockedRequest = MockedRequest,
> extends RestHandler<T> {
	constructor(
		private request: AdapterRequestArgs,
		response: SafeResponse | AsyncResponseResolver<T>,
	) {
		super(
			request.method,
			new URL(request.path.split('?')[0], 'http://localhost').toString(),
			async (
				req: T,
				res,
				ctx,
			): Promise<ResponseResolverReturnType<MockedResponse<any>>> => {
				if ('body' in request) {
					if (!deepEqual(request.body, await req.json())) {
						this.shouldSkip = true;
						return res();
					}
				} else if (req.bodyUsed) {
					this.shouldSkip = true;
					return res();
				}
				// TODO: request headers aren't checked

				if (typeof response === 'function') return response(req, res, ctx);
				return toMswResponse(response, res, ctx);
			},
		);
	}

	override run(
		...params: Parameters<RestHandler<T>['run']>
	): ReturnType<RestHandler<T>['run']> {
		this.shouldSkip = false;
		return super.run(...params);
	}

	override predicate(request: T, parsedResult: Match): boolean {
		// body isn't checked here because this can't be async, so instead it is checked in the MappedRestHandler above
		return (
			super.predicate(request, parsedResult) && this.queryStringMatch(request)
		);
	}

	private queryStringMatch(request: T) {
		return (
			request.url.searchParams.toString() ==
			(this.request.path.split('?')[1] ?? '')
		);
	}
}

export function toMswHandler<
	TMethod extends HttpMethod,
	TUrlParams extends {},
	TRequestParams extends TUrlParams,
	TRequestBodies extends RequestBodies,
	TResponses extends StandardResponse,
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
) {
	type MyRequest =
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

	return function (
		request: MyRequest,
		response: AsyncResponseResolver | SafeResponse<TResponses>,
	) {
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
		return new MappedRestHandler(standardRequest, response);
	};
}
