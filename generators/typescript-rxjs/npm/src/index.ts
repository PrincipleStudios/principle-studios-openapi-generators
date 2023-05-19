import type {
	RequestBodies,
	RequestConversion,
	StandardResponse,
	TransformCallType,
	AdapterRequestArgs,
	RequestConversions,
	HttpMethod
} from '@principlestudios/openapi-codegen-typescript';

import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ajax, AjaxError, AjaxRequest, AjaxResponse } from 'rxjs/ajax';

export const toUrl = (prefix: string, requestOpts: AdapterRequestArgs) =>
	`${prefix}${requestOpts.path}`;

function rxWithPrefix(prefix: string, rxjsRequest: (params: AjaxRequest) => Observable<AjaxResponse> = ajax) {
	const createRequestArgs = (requestOpts: AdapterRequestArgs): AjaxRequest => {
		const url = toUrl(prefix, requestOpts);

		return {
			url,
			method: requestOpts.method,
			headers: requestOpts.headers,
			body:
				requestOpts.headers && requestOpts.headers['Content-Type'] === 'application/x-www-form-urlencoded'
					? requestOpts.body
					: JSON.stringify(requestOpts.body),
			responseType: 'json',
		};
	};

	return function fetchRequest<
		TParams extends {},
		TBody extends RequestBodies,
		TResponse extends StandardResponse,
		TCallType extends TransformCallType
	>(conversion: RequestConversion<HttpMethod, any, TParams, TBody, TResponse, TCallType>) {
		function transform({ params = {}, body = undefined, mimeType = undefined }: any = {}): Observable<TResponse> {
			const requestOpts: AdapterRequestArgs = conversion.request(
				params,
				body,
				mimeType || (body ? 'application/json' : undefined)
			);
			return rxjsRequest(createRequestArgs(requestOpts)).pipe(
				catchError((ex: AjaxError) => of(ex)),
				map((response) =>
					conversion.response({
						status: response.status,
						response: response.response,
						getResponseHeader(header) {
							return response.xhr.getResponseHeader(header);
						},
					})
				)
			);
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
		? <Mime extends keyof TBodies>(req?: RequestParam<TCallType, TParams, TBodies, Mime>) => Observable<TResponse>
		: <Mime extends keyof TBodies>(req: RequestParam<TCallType, TParams, TBodies, Mime>) => Observable<TResponse>
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

export function toRxjsApi<TMethods extends RequestConversions>(
	api: TMethods,
	prefix = '',
	rxjsRequest: (params: AjaxRequest) => Observable<AjaxResponse> = ajax
) {
	return applyTransform(api, rxWithPrefix(prefix, rxjsRequest));
}
