import allOperations from "./no-refs/operations";
import { conversion as lookupRecord, RequestBodies as LookupRecordRequestBodies, Responses as LookupResponses } from "./no-refs/operations/lookupRecord";
import { conversion as getPhoto } from "./no-refs/operations/getPhoto";
import { RequestConversion, RequestBodies, RequestConversions, TransformRequest, TransformResponse } from "~/src/types";
import { AdapterRequestArgs } from "~/src/inputs-outputs";

describe('no-refs', () => {
    it('has successful typings', () => {
        lookupRecord.request({}, { formattedAddress: '123 Main St', location: { latitude: 0, longitude: 0 }}, 'application/json');
        // lookupRecord.request({});
        getPhoto.request({ id: 'foo' });
    });
});

const t: TransformRequest<Object, LookupRecordRequestBodies, 'body', AdapterRequestArgs> = lookupRecord.request;
const t2: RequestConversion<any, Object, Object, LookupRecordRequestBodies, LookupResponses, 'body'> = lookupRecord;
const t3: TransformRequest<any, never, 'no-body', AdapterRequestArgs> = getPhoto.request;

type Req = typeof lookupRecord extends RequestConversion<any, any, any, infer Requests, any, 'body'> ? Requests : never;

const temp = {
    lookupRecord,
    getPhoto,
} as const;

const emptyExtendsRequestBodies: {} extends RequestBodies ? true : false = true;

const extendsRequestConversions: typeof temp extends RequestConversions ? true : false = true;
const allOperationsExtendsRequestConversions: typeof allOperations extends RequestConversions ? true : false = true;
