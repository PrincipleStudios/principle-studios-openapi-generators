import { RequestBodies, RequestConversion, StandardResponse, TransformCallType, TransformRequest, RequestOpts, RequestConversions, HttpQuery, encodeURI } from '@principlestudios/openapi-codegen-typescript';

import { Observable, Subject, Subscriber, merge } from 'rxjs';
import { map } from 'rxjs/operators';
import { ajax, AjaxRequest, AjaxResponse } from 'rxjs/ajax';

const queryString = (params: HttpQuery): string => Object.keys(params)
    .map((key) => {
        const value = params[key];
        return (value instanceof Array)
            ? value.map((val) => `${encodeURI(key)}=${encodeURI(val)}`).join('&')
            : `${encodeURI(key)}=${encodeURI(value)}`;
    })
    .join('&');

function rxWithPrefix(prefix: string, rxjsRequest: (params: AjaxRequest) => Observable<AjaxResponse> = ajax) {

    const createRequestArgs = (requestOpts: RequestOpts): AjaxRequest => {
        let url = prefix + requestOpts.path;
        if (requestOpts.query !== undefined && Object.keys(requestOpts.query).length !== 0) {
            // only add the queryString to the URL if there are query parameters.
            // this is done to avoid urls ending with a '?' character which buggy webservers
            // do not handle correctly sometimes.
            url += '?' + queryString(requestOpts.query);
        }

        return {
            url,
            method: requestOpts.method,
            headers: requestOpts.headers,
            body: (requestOpts.headers && requestOpts.headers['Content-Type'] === 'application/x-www-form-urlencoded') ? requestOpts.body : JSON.stringify(requestOpts.body),
            responseType: requestOpts.responseType || 'json',
        };
    }

    return function fetchRequest<TParams extends {}, TBody extends RequestBodies, TResponse extends StandardResponse, TCallType extends TransformCallType>(
        conversion: RequestConversion<TParams, TBody, TResponse, TCallType>
    ): TransformRequest<TParams, TBody, TCallType, Observable<TResponse>> {

        function transform(params: TParams): Observable<TResponse>;
        function transform<K extends keyof TBody>(params: TParams, body: TBody[K], mimeType: K): Observable<TResponse>;
        function transform(params: TParams, body: TBody[any] | null = null, mimeType: string | null = null): Observable<TResponse> {
            const requestOpts: RequestOpts = conversion.request(params, body!, mimeType!);
            return rxjsRequest(createRequestArgs(requestOpts))
                .pipe(map(response => conversion.response({
                    status: response.status,
                    response: response.response,
                    getResponseHeader(header) { return response.xhr.getResponseHeader(header); },
                })));
        }
        return transform as TransformRequest<TParams, TBody, TCallType, Observable<TResponse>>;
    }
}

type Converted<TConversion extends RequestConversion<any, any, any, any>> = TConversion extends RequestConversion<infer TParams, infer TBody, infer TResponse, infer TCallType> ? TransformRequest<TParams, TBody, TCallType, Observable<TResponse>> : never;

function applyTransform<TMethods extends RequestConversions>(
    methods: TMethods,
    transform: (input: RequestConversion<any, any, any, any>) => Converted<RequestConversion<any, any, any, any>>
): {
        [K in keyof TMethods]: Converted<TMethods[K]>;
    } {
    return Object.keys(methods).reduce((prev, next) => ({ ...prev, [next]: transform(methods[next]) }), {} as {
        [K in keyof TMethods]: Converted<TMethods[K]>;
    });
}

export function toRxjsApi<TMethods extends RequestConversions>(api: TMethods, prefix: string = '', rxjsRequest: (params: AjaxRequest) => Observable<AjaxResponse> = ajax) {
    return applyTransform(api, rxWithPrefix(prefix, rxjsRequest));
}
