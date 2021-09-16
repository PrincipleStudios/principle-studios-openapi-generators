import { RequestOpts, ResponseArgs } from "./inputs-outputs";

export type RequestBodies = {
    [mimeType: string]: unknown;
    'application/x-www-form-urlencoded': Object;
}

export type Responses = {
    'other'?: {
        [mimeType: string]: unknown;
    };
    [statusCode: number]: {
        [mimeType: string]: unknown;
    }
}

export type StandardResponse<TStatusCode extends number | 'other' = number | 'other', TMimeType extends string = string, TBody extends unknown = unknown> =
    { statusCode: TStatusCode; mimeType: TMimeType; body: TBody };

export type TransformRequestNoBody<TRequestParams extends {}> = (params: TRequestParams) => RequestOpts;
export type TransformRequestWithBody<TRequestParams extends {}, TRequestBodies extends RequestBodies> =
    (params: TRequestParams, body: TRequestBodies[keyof TRequestBodies], mimeType: keyof TRequestBodies) => RequestOpts;
export type TransformResponse<TResponses extends Responses> = (args: ResponseArgs) => TResponses;

export type RequestConversion<TRequestParams extends {}, TRequestBodies extends RequestBodies, TResponses extends Responses> = {
    request: (TransformRequestNoBody<TRequestParams> | never) & (TransformRequestWithBody<TRequestParams, TRequestBodies> | never);
    response: (args: ResponseArgs) => TResponses;
};

export type RequestConversions = Record<string, RequestConversion<{}, RequestBodies, Responses>>;
