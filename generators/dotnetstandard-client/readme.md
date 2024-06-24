# OpenAPI Codegen for .NET Standard HttpClient

See the Readme in /generators/dotnetcore-server-interfaces/OpenApiCodegen.Server.Mvc for usage details.

## Testing locally

1. Add `Debugger.Launch();` into the generator to ensure you get prompted to attach the debugger
2. Run:

        dotnet build-server shutdown

3. Run:

        dotnet build examples\dotnetstandard-client\ClientInterfacesExample\ClientInterfacesExample.csproj -p:UseProjectReferences=true --no-incremental

You must repeat step 2 each time the code changes; this should detatch your debugger.

Consider:

- Adding binary log to the build command and use the [MSBuild Binary and Structured Log Viewer](https://msbuildlog.com/)

        dotnet build examples\dotnetstandard-client\ClientInterfacesExample\ClientInterfacesExample.csproj -bl:..\binlogs\client-examples.binlog --no-incremental -p:UseProjectReferences=true; start ..\binlogs\client-examples.binlog

