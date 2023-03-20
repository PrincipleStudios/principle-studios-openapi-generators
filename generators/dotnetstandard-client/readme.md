# OpenAPI Codegen for .NET Standard HttpClient

## Testing locally

1. Add `Debugger.Launch();` into the generator to ensure you get prompted to attach the debugger
2. Run `dotnet build-server shutdown`
3. Run `dotnet build examples\dotnetstandard-client\PrincipleStudios.ClientInterfacesExample\PrincipleStudios.ClientInterfacesExample.csproj --no-incremental`

You must repeat step 2 each time the code changes; this should detatch your debugger.

Consider:

- Adding binary log to the build command.

        dotnet build examples\dotnetstandard-client\PrincipleStudios.ClientInterfacesExample\PrincipleStudios.ClientInterfacesExample.csproj -bl:..\binlogs\client-examples.binlog --no-incremental; start ..\binlogs\client-examples.binlog

