
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

[1]: https://github.com/microsoft/OpenAPI.NET