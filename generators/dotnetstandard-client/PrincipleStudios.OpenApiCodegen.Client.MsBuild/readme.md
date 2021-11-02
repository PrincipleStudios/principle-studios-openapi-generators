Adds source generators to generate C# client extension methods from an OpenAPI specification file.

Add this package, select the OpenAPI specification file from your project, and set the build action to `OpenApiSchemaClient`. Within that folder's namespace, you'll automatically get the model and extension methods to invoke the API.

This integrates during the build phase, so you can be sure your classes are up to date with your schema documentation. (You may need to unload/reload your project in Visual Studio.)

Depends on Newtonsoft.Json for attributes and serialization control. Be sure to include the version you need; this package can use any version.