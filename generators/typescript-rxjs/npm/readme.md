# OpenAPI Codegen for a TypeScript-friendly RxJS client

Provides a conversion method for
[@principlestudios/openapi-codegen-typescript][1] to integrate with rxjs.

    npm i -D @principlestudios/openapi-codegen-typescript-rxjs

You must also have `dotnet` 7.0 runtime installed on your machine.

This will provide a corresponding bin to generate the typescript files.

    openapi-codegen-typescript api.yaml api-generated/ -c

You can then create an API wrapper such as:

    import { toRxjsApi } from '@principlestudios/openapi-codegen-typescript-rxjs';
    import operations from './api-generated/operations';

    export default toRxjsApi(operations);

This API will use the type safety from OpenAPI along with rxjs.

[1]: https://www.npmjs.com/package/@principlestudios/openapi-codegen-typescript