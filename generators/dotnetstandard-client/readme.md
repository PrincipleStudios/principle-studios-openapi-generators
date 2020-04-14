# OpenAPI Codegen for .NET Core Server Interfaces

Uses [OpenAPITools/openapi-generator][1] along with custom templates and a nuget
package in order to generate MVC Controller abstract classes and model classes
to match the Open API specification.

Adding the nuget package `PrincipleStudios.OpenApiCodegen.Server.Mvc` to your
project will enable a new "Build Action" on files in that project called
"OpenApiSchema". (Versions of Visual Studio older than 2019 may need to be
restarted after adding the package.) Setting a valid OpenAPI schema file (YAML
or JSON), up to v3, will generate new classes within the project. (These classes
will be in the namespace according to where the file is located.) Implementing
the controllers in a project with default `UseMvc()` configuration will allow
the project to function as an OpenAPI server for that API.

This package can be marked as a "development only" package in your nuspec if you
are planning on re-publishing this as a library. See [Package references][2] for
more information.

_Note:_ Not implementing the classes will allow you to share a DLL that would
act as the starting point for a server.

# Working with this source code

This packages a java .jar file with the templates, tests via Jest (to be
consistent with the snapshot tests we do elsewhere in this repository), and
delivers via NuGet (for easy use for the C# projects for which this is
designed.)

Prerequisites:

    JDK (We prefer OpenJDK)
    Node/npm
    Dotnot CLI v2 or higher

To build:

    cd nuget
    dotnet pack

To update Jest snapshots: (pay attention to slash types, it's important)

    cd java
    ..\..\tools\gradlew jar
    cd ../npm
    npm test -- -u

To clean:

    cd nuget
    dotnet clean

[1]: https://github.com/OpenAPITools/openapi-generator
[2]: https://docs.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files