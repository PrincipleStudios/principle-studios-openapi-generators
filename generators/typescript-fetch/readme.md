# OpenAPI Codegen for a TypeScript-friendly Fetch client

Provides an adapter layer method for
[@principlestudios/openapi-codegen-typescript][1] to integrate with fetch.

    npm i -D @principlestudios/openapi-codegen-typescript-fetch

You must also have `dotnet` 7.0 runtime installed on your machine.

This will provide a corresponding bin to generate the typescript files.

    openapi-codegen-typescript api.yaml api-generated/ -c

You can then create an API wrapper such as:

    import { toFetchApi } from '@principlestudios/openapi-codegen-typescript-fetch';
    import operations from './api-generated/operations';

    export default toFetchApi(operations, fetch);

This API will use the type safety from OpenAPI along with `fetch`.

Optionally, a third parameter may be passed to `toFetchApi` to prefix all URLs; do not include the trailing `/`. For instance, if the API is located at `/api/example`, but the OpenAPI endpoint is just `/example`, use `toFetchApi(operations, fetch, '/api')`.

## Use with `node-fetch`

To use with `node-fetch`, the prefix specified must be a fully-qualified URL. For instance, if the API is located at `https://server/api/example`, but the OpenAPI endpoint is just `/example`, use `toFetchApi(operations, fetch, 'https://server/api')`. In addition, if you do not have the "DOM" lib specified in your tsconfig.json, make the following changes:

1. Ensure you have a compatible version of `node-fetch` installed.
2. Add a `types.d.ts` file (or other `.d.ts` file to be picked up by TypeScript in your Node sections) to your project with the following global type declarations:

    ```typescript
    type FormData = typeof import('formdata-polyfill/esm.min.js').FormData;
    type Headers = import('node-fetch').Headers;
    type Blob = NodeJS.ReadableStream;
    ```

[1]: https://www.npmjs.com/package/@principlestudios/openapi-codegen-typescript