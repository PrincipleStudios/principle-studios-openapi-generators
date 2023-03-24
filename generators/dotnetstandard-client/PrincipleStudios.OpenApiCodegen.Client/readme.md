Adds source generators to generate C# client extension methods from an OpenAPI specification file.

Add this package, select the OpenAPI specification file from your project, and set the build action to `OpenApiSchemaClient`. Within that folder's namespace, you'll automatically get the model and extension methods to invoke the API.

This integrates during the build phase, so you can be sure your classes are up to date with your schema documentation.

Requirements:

- C# 11+
- .NET 7
- Roslyn 4.0+ (VS 2022 or later, or other up-to-date Roslyn installation.)
