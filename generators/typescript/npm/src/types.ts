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

export type TransformRequestNoBody<TRequestParams extends {},> = (params: TRequestParams) => RequestOpts;
export type TransformRequestWithBody<TRequestParams extends {}, TRequestBodies extends RequestBodies> =
    <K extends keyof TRequestBodies>(params: TRequestParams, body: TRequestBodies[K], mimeType: K) => RequestOpts;
export type TransformRequestWithOptionalBody<TRequestParams extends {}, TRequestBodies extends RequestBodies> =
    TransformRequestNoBody<TRequestParams> & TransformRequestWithBody<TRequestParams, TRequestBodies>;

export type TransformCallType = 'no-body' | 'body' | 'optional';

export type TransformRequest<TRequestParams extends {}, TRequestBodies extends RequestBodies, TCallType extends TransformCallType> =
    TCallType extends 'no-body' ? TransformRequestNoBody<TRequestParams>
    : TCallType extends 'body' ? TransformRequestWithBody<TRequestParams, TRequestBodies>
    : TCallType extends 'optional' ? TransformRequestWithOptionalBody<TRequestParams, TRequestBodies>
    : never;

export type TransformResponse<TResponses extends Responses> = (args: ResponseArgs) => TResponses;

export type RequestConversion<TRequestParams extends {}, TRequestBodies extends RequestBodies, TResponses extends Responses, TCallType extends TransformCallType> = {
    request: TransformRequest<TRequestParams, TRequestBodies, TCallType>;
    response: (args: ResponseArgs) => TResponses;
};

export type RequestConversions = Record<string, RequestConversion<any, any, Responses, TransformCallType>>;
