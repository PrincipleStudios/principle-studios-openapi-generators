import { describe, it } from 'vitest';
import type allOperations from './no-refs/operations';
import { conversion as getPhoto } from './no-refs/operations/getPhoto';
import type {
	RequestBodies as LookupRecordRequestBodies,
	Responses as LookupResponses,
} from './no-refs/operations/lookupRecord';
import { conversion as lookupRecord } from './no-refs/operations/lookupRecord';
import type { AdapterRequestArgs } from '~/src/inputs-outputs';
import type {
	RequestConversion,
	RequestBodies,
	RequestConversions,
	TransformRequest,
} from '~/src/types';

describe('no-refs', () => {
	it('has successful typings', () => {
		lookupRecord.request(
			{},
			{
				formattedAddress: '123 Main St',
				location: { latitude: 0, longitude: 0 },
			},
			'application/json',
		);
		// lookupRecord.request({});
		getPhoto.request({ id: 'foo' });
	});
});

lookupRecord.request satisfies TransformRequest<
	NonNullable<unknown>,
	LookupRecordRequestBodies,
	'body',
	AdapterRequestArgs
>;
lookupRecord satisfies RequestConversion<
	any,
	Object,
	Object,
	LookupRecordRequestBodies,
	LookupResponses,
	'body'
>;
getPhoto.request satisfies TransformRequest<
	any,
	never,
	'no-body',
	AdapterRequestArgs
>;

const temp = {
	lookupRecord,
	getPhoto,
} as const;

true satisfies {} extends RequestBodies ? true : false;
true satisfies typeof temp extends RequestConversions ? true : false;
true satisfies typeof allOperations extends RequestConversions ? true : false;
