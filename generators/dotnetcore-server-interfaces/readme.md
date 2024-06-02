# OpenAPI Codegen for .NET Core Server Interfaces

See the Readme in /generators/dotnetcore-server-interfaces/PrincipleStudios.OpenApiCodegen.Server.Mvc for usage details.

## Working with this source code

Prerequisites:

    .NET 8.0 SDK

## Testing locally

1. Add `Debugger.Launch();` into the generator to ensure you get prompted to attach the debugger
2. Run:

        dotnet build-server shutdown

3. Run one of the following:

        dotnet build examples\dotnetcore-server-interfaces\PrincipleStudios.ServerInterfacesExample\PrincipleStudios.ServerInterfacesExample.csproj -p:UseProjectReferences=true --no-incremental
        dotnet build examples\dotnetcore-server-interfaces\PrincipleStudios.ServerInterfacesExample.Oauth\PrincipleStudios.ServerInterfacesExample.Oauth.csproj -p:UseProjectReferences=true --no-incremental

You must repeat step 2 each time the code changes; this should detatch your debugger.

Consider:

- Adding binary log to the build command and use the [MSBuild Binary and Structured Log Viewer](https://msbuildlog.com/)

        dotnet build examples\dotnetcore-server-interfaces\PrincipleStudios.ServerInterfacesExample\PrincipleStudios.ServerInterfacesExample.csproj -bl:..\binlogs\server-examples.binlog --no-incremental -p:UseProjectReferences=true; start ..\binlogs\server-examples.binlog
        dotnet build examples\dotnetcore-server-interfaces\PrincipleStudios.ServerInterfacesExample.Oauth\PrincipleStudios.ServerInterfacesExample.Oauth.csproj -p:UseProjectReferences=true --no-incremental -bl:..\binlogs\server-examples-oauth.binlog --no-incremental -p:UseProjectReferences=true; start ..\binlogs\server-examples-oauth.binlog


[1]: https://github.com/microsoft/OpenAPI.NET
[2]: https://docs.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files