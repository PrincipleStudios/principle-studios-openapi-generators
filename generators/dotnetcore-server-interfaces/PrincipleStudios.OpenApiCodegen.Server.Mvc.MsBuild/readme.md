Adds source generators to generate C# controller base classes from an OpenAPI specification file.

Add this package, select the OpenAPI specification file from your project, and set the build action to `OpenApiSchemaMvcServer`. Within that folder's namespace, you'll automatically get the model and controller classes to implement your interface.

A `services.AddOpenApi...` extension method is added for use in your startup file to ensure you have all base controllers implemented.

This integrates during the build phase, so you can be sure your classes are up to date with your schema documentation. (You may need to unload/reload your project in Visual Studio.)

Depends on Newtonsoft.Json for attributes and serialization control. Be sure to include the version you need; this package can use any version.