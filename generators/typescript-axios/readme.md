# OpenAPI Codegen for a TypeScript-friendly Axios client

Uses [OpenAPITools/openapi-generator][1] along with custom templates in order to
generate a TypeScript Axios client. Delivered via an npm package.

    npm i -D @principlestudios/openapi-codegen-typescript-axios

This will also include `@openapitools/openapi-generator-cli`, which adds a bin.

    openapi-generator generate -i api.yaml -o api-generated/ -g typescript-axios -t node_modules/@principlestudios/openapi-codegen-typescript-axios/templates

The above example will take as input an `api.yaml`, output an `api-generated/`
folder with all the typescript files and a gitignore. You should add this as
part of your CI process with only the OpenAPI spec checked in, or download the
spec as part of the CI process.

# Working with this source code

This packages only new templates, tests via Jest (to be consistent with
the snapshot tests we do elsewhere in this repository), and delivers via npm (for easy
use for the C# projects for which this is designed) to work with the existing
`@openapitools/openapi-generator-cli` npm package.

Prerequisites:

    Node/npm

No build needed!

To update Jest snapshots: (pay attention to slash types, it's important)

    npm test -- -u

To package:

    npm pack

[1]: https://github.com/OpenAPITools/openapi-generator