FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-dotnet
WORKDIR /src

COPY ./global.json ./
COPY ./Directory.Build.props ./
COPY ./examples/dotnetcore-server-interfaces/dotnetcore-server-interfaces.sln .
COPY ./examples/dotnetcore-server-interfaces/PrincipleStudios.ServerInterfacesExample/PrincipleStudios.ServerInterfacesExample.csproj ./PrincipleStudios.ServerInterfacesExample/
COPY ./examples/dotnetcore-server-interfaces/PrincipleStudios.ServerInterfacesExample.Oauth/PrincipleStudios.ServerInterfacesExample.Oauth.csproj ./PrincipleStudios.ServerInterfacesExample.Oauth/
ARG VersionSuffix
COPY ./examples/dotnetcore-server-interfaces/tooling/NuGet.Sample.Config ./NuGet.Config
ARG GitHubToken
RUN sed -i "s/--your-key--/${GitHubToken}/" NuGet.Config
RUN dotnet restore -p VersionSuffix=${VersionSuffix} \
                   -p TreatWarningsAsErrors=true

COPY ./schemas/ ./schemas/
COPY ./examples/dotnetcore-server-interfaces/PrincipleStudios.ServerInterfacesExample/ ./PrincipleStudios.ServerInterfacesExample/
COPY ./examples/dotnetcore-server-interfaces/PrincipleStudios.ServerInterfacesExample.Oauth/ ./PrincipleStudios.ServerInterfacesExample.Oauth/
RUN dotnet build --no-restore -p VersionSuffix=${VersionSuffix}
