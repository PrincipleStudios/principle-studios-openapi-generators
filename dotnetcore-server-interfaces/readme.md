OpenAPI generator for ASP.Net Core Interfaces.

To build:

    call ../gradlew shadowJar

To generate:

    java -cp build\libs\default-openapi-generator-1.0.0-all.jar org.openapitools.codegen.OpenAPIGenerator generate -i <openapi-file.yaml> -o <output-folder> -g dotnetcore-interfaces
