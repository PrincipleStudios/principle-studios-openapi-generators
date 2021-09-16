import { RequestOpts, ResponseArgs } from "./inputs-outputs";

export type RequestBodies = {
    [mimeType: string]: any;
}

export type Responses = {
    'other'?: {
        [mimeType: string]: any;
    };
    [statusCode: number]: {
        [mimeType: string]: any;
    }
}

export type StandardResponse<TStatusCode extends number | 'other' = number | 'other', TMimeType extends string = string, TBody extends unknown = unknown> =
    { statusCode: TStatusCode; mimeType: TMimeType; data: TBody; response: ResponseArgs };

export type TransformRequestNoBody<TRequestParams extends {}, TResult> = (params: TRequestParams) => TResult;
export type TransformRequestWithBody<TRequestParams extends {}, TRequestBodies extends RequestBodies, TResult> =
    <K extends keyof TRequestBodies>(params: TRequestParams, body: TRequestBodies[K], mimeType: K) => TResult;
export type TransformRequestWithOptionalBody<TRequestParams extends {}, TRequestBodies extends RequestBodies, TResult> =
    TransformRequestNoBody<TRequestParams, TResult> & TransformRequestWithBody<TRequestParams, TRequestBodies, TResult>;

export type TransformCallType = 'no-body' | 'body' | 'optional';

export type TransformRequest<TRequestParams extends {}, TRequestBodies extends RequestBodies, TCallType extends TransformCallType, TResult> =
    TCallType extends 'no-body' ? TransformRequestNoBody<TRequestParams, TResult>
    : TCallType extends 'body' ? TransformRequestWithBody<TRequestParams, TRequestBodies, TResult>
    : TCallType extends 'optional' ? TransformRequestWithOptionalBody<TRequestParams, TRequestBodies, TResult>
    : never;

export type TransformResponse<TResponses extends StandardResponse> = (args: ResponseArgs) => TResponses;

export type RequestConversion<TRequestParams extends {}, TRequestBodies extends RequestBodies, TResponses extends StandardResponse, TCallType extends TransformCallType> = {
    request: TransformRequest<TRequestParams, TRequestBodies, TCallType, RequestOpts>;
    response: TransformResponse<TResponses>;
};

export type RequestConversions = Record<string, RequestConversion<any, any, StandardResponse, TransformCallType>>;
