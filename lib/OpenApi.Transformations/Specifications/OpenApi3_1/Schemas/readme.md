# Open API schema specifications for v3.1

Source: https://github.com/OAI/OpenAPI-Specification/tree/main/schemas/v3.1

- *schema-base.yaml* - base schema for all of 3.1

# Sechemas not included:

- [https://spec.openapis.org/oas/3.1/schema/2022-10-07](https://github.com/OAI/OpenAPI-Specification/blob/main/schemas/v3.1/schema.yaml) - JSON Schema document to validate an OpenAPI 3.1.0
	- This is already parsed at `Json.Schema.OpenApi.MetaSchemas.DocumentSchema`
- [https://spec.openapis.org/oas/3.1/dialect/base](https://github.com/OAI/OpenAPI-Specification/blob/main/schemas/v3.1/dialect/base.schema.yaml) - Dialect-schema specifying the base vocabulary for the JSON Schema
	- This is already parsed at `Json.Schema.OpenApi.MetaSchemas.OpenApiDialect`
- [https://spec.openapis.org/oas/3.1/meta/base](https://github.com/OAI/OpenAPI-Specification/blob/main/schemas/v3.1/meta/base.schema.yaml) - Meta-schema extending the vocabulary for JSON Schema
	- This is already parsed at `Json.Schema.OpenApi.MetaSchemas.OpenApiMeta`
