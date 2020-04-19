OpenAPI generator for ASP.Net Core Interfaces.

To install:

    npm i -D @principlestudios/openapi-codegen-dotnetstandard-client-interfaces

To generate, add this to your npm scripts:

    "generateDotNet": "openapi-generator-dotnet generate -g com.principlestudios.codegen.DotNetStandardClientGenerator -o outDir -i ../schemas/petstore.yaml"

Then:

    npm run generateDotNet
