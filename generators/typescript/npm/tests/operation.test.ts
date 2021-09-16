import allOperations from "./no-refs/operations";
import { conversion as lookupRecord, RequestBodies as LookupRecordRequestBodies, StructuredResponses, Responses } from "./no-refs/operations/lookupRecord";
import { conversion as getPhoto } from "./no-refs/operations/getPhoto";
import { RequestConversion, RequestBodies, RequestConversions, TransformRequest, StandardResponse } from "./types";

lookupRecord.request({}, { formattedAddress: '123 Main St', location: { latitude: 0, longitude: 0 }}, 'application/json');
// lookupRecord.request({});
getPhoto.request({ id: 'foo' });

const t: TransformRequest<Object, LookupRecordRequestBodies, 'body'> = lookupRecord.request;
const t2: RequestConversion<Object, LookupRecordRequestBodies, Responses, 'body'> = lookupRecord;
const t3: TransformRequest<any, never, 'no-body'> = getPhoto.request;

type Req = typeof lookupRecord extends RequestConversion<any, infer Requests, any, 'body'> ? Requests : never;

const temp = {
    lookupRecord,
    getPhoto,
} as const;

const emptyExtendsRequestBodies: {} extends RequestBodies ? true : false = true;

const extendsRequestConversions: typeof temp extends RequestConversions ? true : false = true;
const allOperationsExtendsRequestConversions: typeof allOperations extends RequestConversions ? true : false = true;

type DestructureMime<TStatusCode extends number | 'other', T extends { [mimeType: string]: any }> =  { [K in keyof T]: K extends string ? StandardResponse<TStatusCode, K, T[K]> : never }[keyof T];
type Destructure<T extends { [statusCode: string | number]: { [mimeType: string]: any } }> = { [K in keyof T]: K extends number | 'other' ? DestructureMime<K, T[K]> : never }[keyof T];

type Destructured = Destructure<StructuredResponses>;

// So, it is possible to destructure the response, but... this could end up with complex (therefore slow) type checking. I'd rather not.
const d: Destructured = { statusCode: 409, mimeType: 'application/json', data: { multiple: {} as any }, response: {} as any };

