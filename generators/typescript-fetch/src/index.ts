import type {
	RequestBodies,
	RequestConversion,
	StandardResponse,
	TransformCallType,
	AdapterRequestArgs,
	RequestConversions,
	HttpMethod,
	AdapterResponseArgs,
} from '@principlestudios/openapi-codegen-typescript';

const applicationJson = 'application/json';

type AnyObject = Record<string, any>;
type EmptyObject = Record<never, never>;
type AnyRequestConversion = RequestConversion<
	HttpMethod,
	AnyObject,
	AnyObject,
	RequestBodies,
	StandardResponse,
	TransformCallType
>;
type IfKeyless<T, TTrue, TFalse> = EmptyObject extends T ? TTrue : TFalse;

type FetchRequest = {
	method: HttpMethod;
	headers: Record<string, string> | undefined;
	body: FormData | string;
};

type FetchResponse = {
	status: number;
	headers: Headers;
	json(): Promise<unknown>;
	body: unknown; // ReadableStream, but may be different for Node vs DOM
};

export type BaseFetchImplementation = (url: string | URL, requestInit: FetchRequest) => Promise<FetchResponse>;
export type FetchImplementation<TExtra> = (
	...params: [...Parameters<BaseFetchImplementation>, TExtra]
) => Promise<FetchResponse>;

function createRequestArgs(requestOpts: AdapterRequestArgs): Parameters<BaseFetchImplementation> {
	const url = requestOpts.path;

	return [
		url,
		{
			method: requestOpts.method,
			headers: requestOpts.headers
				? Object.fromEntries(Object.entries(requestOpts.headers).filter((t): t is [string, string] => t[1] !== null))
				: undefined,
			body:
				requestOpts.headers && requestOpts.headers['Content-Type'] === 'application/x-www-form-urlencoded'
					? (requestOpts.body as FormData)
					: JSON.stringify(requestOpts.body),
		},
	];
}

async function createResponseArgs(
	fetchResponse: Awaited<ReturnType<BaseFetchImplementation>>
): Promise<AdapterResponseArgs> {
	const response = await fetchResponse;
	const contentType = response.headers.get('Content-Type') ?? '';
	return {
		status: response.status,
		response: contentType.split(';')[0] === applicationJson ? await response.json() : response.body,
		getResponseHeader(header) {
			return response.headers.get(header);
		},
	};
}

export function toFetchOperation<
	TParams extends AnyObject,
	TBody extends RequestBodies,
	TResponse extends StandardResponse,
	TCallType extends TransformCallType,
	TExtra
>(
	fetchImpl: FetchImplementation<TExtra>,
	conversion: RequestConversion<HttpMethod, AnyObject, TParams, TBody, TResponse, TCallType>
): Converted<AnyRequestConversion, TExtra> {
	return async function transform(...[param]): Promise<TResponse> {
		const { params = {}, body = undefined, extra } = param ?? {};
		const requestArgs: AdapterRequestArgs = conversion.request(
			params as TParams,
			body as TBody['application/json'],
			(body ? applicationJson : undefined) as keyof TBody
		);
		const fetchArgs = createRequestArgs(requestArgs);
		const fetchResponse = await fetchImpl(...fetchArgs, extra as TExtra);
		const adapterResponseArgs = await createResponseArgs(fetchResponse);
		return conversion.response(adapterResponseArgs);
	};
}

type ParamPart<TParams> = IfKeyless<TParams, { params?: TParams }, { params: TParams }>;
type NoBody = { body?: undefined };
type BodyPartInner<TBodies extends RequestBodies> = { body: TBodies['application/json'] };
type BodyPart<TBodies extends RequestBodies, TCallType extends TransformCallType> = TCallType extends 'no-body'
	? NoBody
	: TCallType extends 'optional'
	? BodyPartInner<TBodies> | NoBody
	: BodyPartInner<TBodies>;
type ExtraPart<TExtra> = undefined extends TExtra ? { extra?: TExtra } : { extra: TExtra };

type RequestParam<
	TCallType extends TransformCallType,
	TParams,
	TBodies extends RequestBodies,
	TExtra
> = ParamPart<TParams> & BodyPart<TBodies, TCallType> & ExtraPart<TExtra>;

type ConvertedParams<TCallType extends TransformCallType, TParams, TBodies extends RequestBodies, TExtra> = IfKeyless<
	RequestParam<TCallType, TParams, TBodies, TExtra>,
	[req?: RequestParam<TCallType, TParams, TBodies, TExtra>],
	[req: RequestParam<TCallType, TParams, TBodies, TExtra>]
>;

type Converted<TConversion extends AnyRequestConversion, TExtra> = TConversion extends RequestConversion<
	HttpMethod,
	AnyObject,
	infer TParams,
	infer TBodies,
	infer TResponse,
	infer TCallType
>
	? (...args: ConvertedParams<TCallType, TParams, TBodies, TExtra>) => Promise<TResponse>
	: never;

function applyTransform<TMethods extends RequestConversions, TExtra>(
	methods: TMethods,
	transform: (input: AnyRequestConversion) => Converted<AnyRequestConversion, TExtra>
): {
	[K in keyof TMethods]: Converted<TMethods[K], TExtra>;
} {
	return Object.fromEntries(
		Object.entries(methods).map(([operationId, conversion]) => [operationId, transform(conversion)])
	) as {
		[K in keyof TMethods]: Converted<TMethods[K], TExtra>;
	};
}

export function toFetchApi<TMethods extends RequestConversions, TExtra>(
	api: TMethods,
	fetchImpl: FetchImplementation<TExtra>
) {
	return applyTransform<TMethods, TExtra>(api, (conversion) => toFetchOperation(fetchImpl, conversion));
}
