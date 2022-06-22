# OpenAPI Codegen for a TypeScript-friendly Open API clients

Uses [Microsoft.OpenApi.Readers][1] along with custom templates in order to
generate generalized TypeScript objects for creating clients of many shapes.
Delivered via an npm package.

    npm i -D @principlestudios/openapi-codegen-typescript

You must also have `dotnet` 6.0 runtime installed on your machine.

This will provide a corresponding bin to generate the typescript files.

    openapi-codegen-typescript api.yaml api-generated/ -c

The above example will take as input an `api.yaml`, output an `api-generated/`
folder with all the typescript files and a gitignore. You should add this as
part of your CI process with only the OpenAPI spec checked in, or download the
spec as part of the CI process.

Written in C#.

[1]: https://www.nuget.org/packages/Microsoft.OpenApi.Readers
