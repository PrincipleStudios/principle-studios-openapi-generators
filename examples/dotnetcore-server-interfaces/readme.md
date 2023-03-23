# Server Interfaces examples

This folder contains examples that demonstrate the .NET Server generation capabilities of these generators.

## Building with a specific version

Sometimes, you want to build the example with a specific version, rather than what is in source control. (For example, a package from a PR.) From the command line, run:

    dotnet build /p:OpenApiMvcServerFullVersion=<your-version-here>

## Building entirely locally

    dotnet build /p:UseProjectReferences=true
