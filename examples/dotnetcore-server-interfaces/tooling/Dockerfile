FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-dotnet
WORKDIR /src

COPY ./global.json ./
COPY ./Directory.Build.props ./
COPY ./artifacts/packages/Release/ ./artifacts/packages/Release/
COPY ./examples/dotnetcore-server-interfaces/dotnetcore-server-interfaces.sln .
COPY ./examples/dotnetcore-server-interfaces/ServerInterfacesExample/ServerInterfacesExample.csproj ./ServerInterfacesExample/
COPY ./examples/dotnetcore-server-interfaces/ServerInterfacesExample.Oauth/ServerInterfacesExample.Oauth.csproj ./ServerInterfacesExample.Oauth/
COPY ./examples/dotnetcore-server-interfaces/tooling/server-generator-references.props ./examples/dotnetcore-server-interfaces/tooling/
COPY ./examples/dotnetcore-server-interfaces/tooling/NuGet.Sample.Config ./NuGet.Config
ARG VersionSuffix
ARG GitHubToken
RUN dotnet restore -p VersionSuffix=${VersionSuffix} \
                   -p TreatWarningsAsErrors=true \
                   -p UseProjectReferences=false

COPY ./schemas/ ./schemas/
COPY ./examples/dotnetcore-server-interfaces/ServerInterfacesExample/ ./ServerInterfacesExample/
COPY ./examples/dotnetcore-server-interfaces/ServerInterfacesExample.Oauth/ ./ServerInterfacesExample.Oauth/
RUN dotnet build --no-restore -p VersionSuffix=${VersionSuffix} -p UseProjectReferences=false

