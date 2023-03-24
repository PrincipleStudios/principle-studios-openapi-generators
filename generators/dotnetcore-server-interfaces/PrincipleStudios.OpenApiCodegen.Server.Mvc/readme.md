Adds source generators to generate C# controller base classes from an OpenAPI specification file.

Add this package, select the OpenAPI specification file from your project, and set the build action to `OpenApiSchemaMvcServer`. Within that folder's namespace, you'll automatically get the model and controller classes to implement your interface.

A `services.AddOpenApi...` extension method is added for use in your startup file to ensure you have all base controllers implemented.

This integrates during the build phase, so you can be sure your classes are up to date with your schema documentation.

Requirements:

- System.Text.Json
- C# 11+
- .NET 7
- Roslyn 4.0+ (VS 2022 or later, or other up-to-date Roslyn installation.)

This package no longer supports Newtonsoft.Json.
