OpenAPI generator for ASP.Net Core Interfaces.

To install:

    npm i -D @principlestudios/openapi-codegen-dotnetcore-server-interfaces

To generate, add this to your npm scripts:

    "generateDotNet": "openapi-generator-dotnet generate -g com.principlestudios.codegen.DotNetCoreInterfacesGenerator -o outDir -i ../samples/petstore.yaml"

Then:

    npm run generateDotNet
