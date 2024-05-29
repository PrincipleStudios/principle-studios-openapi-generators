import {
	AdapterRequestArgs,
	RequestBodies,
	RequestConversion,
	StandardResponse,
	TransformCallType,
	TransformRequest,
	HttpMethod,
	RequestConversions,
} from '@principlestudios/openapi-codegen-typescript';
import allOperations from './api-generated/operations';
import axios from 'axios';

function fetchWithPrefix(prefix: string) {
	return function fetchRequest<
		TMethod extends HttpMethod,
		TUrlParams extends {},
		TParams extends TUrlParams,
		TBody extends RequestBodies,
		TResponse extends StandardResponse,
		TCallType extends TransformCallType,
	>(
		conversion: RequestConversion<
			TMethod,
			TUrlParams,
			TParams,
			TBody,
			TResponse,
			TCallType
		>,
	): TransformRequest<TParams, TBody, TCallType, Promise<TResponse>> {
		function transform(params: TParams): Promise<TResponse>;
		function transform<K extends keyof TBody>(
			params: TParams,
			body: TBody[K],
			mimeType: K,
		): Promise<TResponse>;
		async function transform(
			params: TParams,
			body: TBody[any] | null = null,
			mimeType: string | null = null,
		): Promise<TResponse> {
			const requestOpts: AdapterRequestArgs = conversion.request(
				params,
				body!,
				mimeType!,
			);

			const axiosResponse = await axios({
				baseURL:
					prefix + requestOpts.path,
				method: requestOpts.method,
				headers: requestOpts.headers,
				data: requestOpts.body,
				responseType: requestOpts.responseType,
			});
			// const response: ResponseArgs = await Promise.reject<ResponseArgs>(); // This is just a demo that always fails
			return conversion.response({
				status: axiosResponse.status,
				response: axiosResponse.data,
				getResponseHeader(header) {
					return axiosResponse.headers[header];
				},
			});
		}
		return transform as TransformRequest<
			TParams,
			TBody,
			TCallType,
			Promise<TResponse>
		>;
	};
}

type Converted<TConversion extends RequestConversion<any, any, any, any, any, any>> =
	TConversion extends RequestConversion<
		any,
		any,
		infer TParams,
		infer TBody,
		infer TResponse,
		infer TCallType
	>
		? TransformRequest<TParams, TBody, TCallType, Promise<TResponse>>
		: never;

function applyTransform<TMethods extends RequestConversions>(
	methods: TMethods,
	transform: (
		input: RequestConversion<any, any, any, any, any, any>,
	) => Converted<RequestConversion<any, any, any, any, any, any>>,
): {
	[K in keyof TMethods]: Converted<TMethods[K]>;
} {
	return Object.keys(methods).reduce(
		(prev, next) => ({ ...prev, [next]: transform(methods[next]) }),
		{} as {
			[K in keyof TMethods]: Converted<TMethods[K]>;
		},
	);
}
export const apiFactory = (urlPrefix: string) =>
	applyTransform(allOperations, fetchWithPrefix(urlPrefix));
