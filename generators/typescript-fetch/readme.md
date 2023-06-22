# OpenAPI Codegen for TypeScript-friendly MSW testing

Provides an adapter layer method for
[@principlestudios/openapi-codegen-typescript][1] to integrate with msw.

    npm i -D @principlestudios/openapi-codegen-typescript-msw

You must also have `dotnet` 7.0 runtime installed on your machine.

This will provide a corresponding bin to generate the typescript files.

    openapi-codegen-typescript api.yaml api-generated/ -c

You can then create a mock MSW service to handle specific requests:

    import { setupServer } from 'msw/node';
    import { toMswHandler } from '@principlestudios/openapi-codegen-typescript-msw';
    import operations from './api-generated/operations';

    const findPets = toMswHandler(operations.findPets);
    const server = setupServer(
        findPets(
            { params: { tags: ['dog','cat'], limit: 10 } },
            { statusCode: 200, data: [{ name: 'Fido', tag: 'dog', id: 1234 }], mimeType: 'application/json' }
        )
    );

This API will use the type safety from OpenAPI along with msw.

[1]: https://www.npmjs.com/package/@principlestudios/openapi-codegen-typescript