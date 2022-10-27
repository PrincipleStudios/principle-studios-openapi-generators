# OpenAPI Codegen for .NET Core Server Interfaces

Uses [OpenAPI.NET][1] with msbuild targets in a nuget package

Adding the nuget package `PrincipleStudios.OpenApiCodegen.Server.Mvc` to your
project will enable a new "Build Action" on files in that project called
"OpenApiSchemaMvcServer". (Versions of Visual Studio older than 2019 may need to be
restarted after adding the package.) Setting a valid OpenAPI schema file (YAML
or JSON), up to v3, will generate new classes within the project. (These classes
will be in the namespace according to where the file is located.) Implementing
the controllers in a project with default `UseMvc()` configuration will allow
the project to function as an OpenAPI server for that API.

A helper method is added along the lines of
`services.AddOpenApi<name-of-api-file>()` is added to help you ensure all
controllers are implemented with any upcoming changes.

This package can be marked as a "development only" package in your nuspec if you
are planning on re-publishing this as a library. See [Package references][2] for
more information.

_Note:_ Not implementing the classes will allow you to share a DLL that would
act as the starting point for a server.

# Working with this source code

Prerequisites:

    .NET CLI 6.0 or higher

To build:

    cd nuget
    dotnet pack

To update snapshots:

* Remove all the snapshots in `/lib/PrincipleStudios.OpenApiCodegen.Server.Mvc.Test/__snapshots__`
* Run the tests

[1]: https://github.com/microsoft/OpenAPI.NET
[2]: https://docs.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files