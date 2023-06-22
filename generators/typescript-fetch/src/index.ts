import type {
	RequestBodies,
	RequestConversion,
	StandardResponse,
	TransformCallType,
	AdapterRequestArgs,
	RequestConversions,
	HttpMethod
} from '@principlestudios/openapi-codegen-typescript';

const applicationJson = 'application/json';

type FetchRequest = {
    method: HttpMethod,
    headers: Record<string, string> | undefined;
    body: FormData | string;
}

type FetchResponse = {
    status: number;
    headers: Headers;
    json(): Promise<unknown>;
    body: unknown; // ReadableStream, but may be different for Node vs DOM
}

type FetchImplementation = (url: URL, requestInit: FetchRequest) => Promise<FetchResponse>;

export const toUrl = (prefix: string, requestOpts: AdapterRequestArgs) =>
	`${prefix}${requestOpts.path}`;

function fetchWithPrefix(prefix: string, fetchImpl: FetchImplementation) {
	const createRequestArgs = (requestOpts: AdapterRequestArgs): [URL, FetchRequest] => {
		const url = toUrl(prefix, requestOpts);

		return [new URL(url), {
			method: requestOpts.method,
			headers: requestOpts.headers ? Object.fromEntries(Object.entries(requestOpts.headers).filter((t): t is [string, string] => t[1] !== null)) : undefined,
			body:
				requestOpts.headers && requestOpts.headers['Content-Type'] === 'application/x-www-form-urlencoded'
					? requestOpts.body as FormData
					: JSON.stringify(requestOpts.body),
		}];
	};

	return function fetchRequest<
		TParams extends {},
		TBody extends RequestBodies,
		TResponse extends StandardResponse,
		TCallType extends TransformCallType
	>(conversion: RequestConversion<HttpMethod, any, TParams, TBody, TResponse, TCallType>) {
		async function transform({ params = {}, body = undefined, mimeType = undefined }: any = {}): Promise<TResponse> {
			const requestOpts: AdapterRequestArgs = conversion.request(
				params,
				body,
				mimeType || (body ? applicationJson : undefined)
			);
			const response = await fetchImpl(...createRequestArgs(requestOpts));

			return conversion.response({
				status: response.status,
				response: response.headers.get('Content-Type') === applicationJson
					? await response.json()
					: response.body,
				getResponseHeader(header) {
					return response.headers.get(header);
				},
			});
		}
		return transform;
	};
}

type ParamPart<TParams> = {} extends TParams ? { params?: TParams } : { params: TParams };
type BodyPartInner<TBodies extends RequestBodies, Mime extends keyof TBodies> = Mime extends 'application/json'
	? { body: TBodies['application/json']; mimeType?: 'application/json' }
	: { body: TBodies[Mime]; mimeType: Mime };
type BodyPart<
	TBodies extends RequestBodies,
	Mime extends keyof TBodies,
	TCallType extends TransformCallType
> = TCallType extends 'no-body'
	? {}
	: TCallType extends 'optional'
	? BodyPartInner<TBodies, Mime> | {}
	: BodyPartInner<TBodies, Mime>;

type RequestParam<
	TCallType extends TransformCallType,
	TParams,
	TBodies extends RequestBodies,
	Mime extends keyof TBodies
> = ParamPart<TParams> & BodyPart<TBodies, Mime, TCallType>;

type Converted<TConversion extends RequestConversion<any, any, any, any, any, any>> = TConversion extends RequestConversion<
	any,
	any,
	infer TParams,
	infer TBodies,
	infer TResponse,
	infer TCallType
>
	? {} extends RequestParam<TCallType, TParams, TBodies, keyof TBodies>
		? <Mime extends keyof TBodies>(req?: RequestParam<TCallType, TParams, TBodies, Mime>) => Promise<TResponse>
		: <Mime extends keyof TBodies>(req: RequestParam<TCallType, TParams, TBodies, Mime>) => Promise<TResponse>
	: never;

function applyTransform<TMethods extends RequestConversions>(
	methods: TMethods,
	transform: (input: RequestConversion<any, any, any, any, any, any>) => Converted<RequestConversion<any, any, any, any, any, any>>
): {
	[K in keyof TMethods]: Converted<TMethods[K]>;
} {
	return Object.keys(methods).reduce(
		(prev, next) => ({ ...prev, [next]: transform(methods[next]) }),
		{} as {
			[K in keyof TMethods]: Converted<TMethods[K]>;
		}
	);
}

export function toFetchApi<TMethods extends RequestConversions>(
	api: TMethods,
	fetchImpl: FetchImplementation,
	prefix = ''
) {
	return applyTransform(api, fetchWithPrefix(prefix, fetchImpl));
}
