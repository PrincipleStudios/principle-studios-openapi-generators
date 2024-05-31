import allOperations from "./no-refs/operations";
import type { Responses as LookupResponses } from "./no-refs/operations/lookupRecord";
import { conversion as lookupRecord } from "./no-refs/operations/lookupRecord";
import type { AdapterRequestArgs, AdapterResponseArgs } from "~/src/inputs-outputs";
import type { RequestConversion, RequestBodies, RequestConversions, TransformRequest, StandardResponse, TransformCallType } from "~/src/types";

function fetchRequest<TParams extends {}, TBody extends RequestBodies, TResponse extends StandardResponse, TCallType extends TransformCallType>(
    conversion: RequestConversion<any, any, TParams, TBody, TResponse, TCallType>
): TransformRequest<TParams, TBody, TCallType, Promise<TResponse>> {

    function transform(params: TParams): Promise<TResponse>;
    function transform<TMimeType extends keyof TBody>(params: TParams, body: TBody[TMimeType], mimeType: TMimeType): Promise<TResponse>;
    async function transform(params: TParams, body: TBody[any] | null = null, mimeType: string | null = null): Promise<TResponse> {
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        const requestOpts: AdapterRequestArgs = conversion.request(params, body as TBody[any], mimeType!);
        const response: AdapterResponseArgs = await Promise.reject<AdapterResponseArgs>(); // This is just a demo that always fails
        return conversion.response(response);
    }
    return transform as TransformRequest<TParams, TBody, TCallType, Promise<TResponse>>;
}
const fetchLookupRecord = fetchRequest(lookupRecord);

type Converted<TConversion extends RequestConversion<any, any, any, any, any, any>> = TConversion extends RequestConversion<any, any, infer TParams, infer TBody, infer TResponse, infer TCallType> ? TransformRequest<TParams, TBody, TCallType, Promise<TResponse>> : never;

function applyTransform<TMethods extends RequestConversions>(
    methods: TMethods,
    transform: (input: RequestConversion<any, any, any, any, any, any>) => Converted<RequestConversion<any, any, any, any, any, any>>
): {
        [K in keyof TMethods]: Converted<TMethods[K]>;
    } {
    return Object.keys(methods).reduce((prev, next) => ({ ...prev, [next]: transform(methods[next]) }), {} as {
        [K in keyof TMethods]: Converted<TMethods[K]>;
    });
}
const fetchTemp = applyTransform(allOperations, fetchRequest);

// eslint-disable-next-line @typescript-eslint/no-unused-vars
async function testPipe() {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const response: LookupResponses = await fetchLookupRecord({}, { formattedAddress: '123 Main St', location: { latitude: 0, longitude: 0 } }, 'application/json');
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const response2: LookupResponses = await fetchTemp.lookupRecord({}, { formattedAddress: '123 Main St', location: { latitude: 0, longitude: 0 } }, 'application/json');
}
