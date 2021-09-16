import { StructuredResponses } from "./no-refs/operations/lookupRecord";
import { StandardResponse } from "~/src/types";

type DestructureMime<TStatusCode extends number | 'other', T extends { [mimeType: string]: any; }> = {
    [K in keyof T]: K extends string ? StandardResponse<TStatusCode, K, T[K]> : never;
}[keyof T];
type Destructure<T extends { [statusCode: string | number]: { [mimeType: string]: any; }; }> = {
    [K in keyof T]: K extends number | 'other' ? DestructureMime<K, T[K]> : never;
}[keyof T];
type Destructured = Destructure<StructuredResponses>;

// So, it is possible to destructure the response, but... this could end up with complex (therefore slow) type checking. I'd rather not.
const d: Destructured = { statusCode: 409, mimeType: 'application/json', data: { multiple: {} as any }, response: {} as any };
