# System.Text.Json refactor

In order to leverage features such as `$ref` to other documents, we'll be
refactoring to use System.Text.Json via [json-everything][json-everything].

## Concepts

[Every element in OpenApi and JSON Schema documents can be referenced by a
URI.][uri-reference] The full URI for a reference may include a URI fragment
(part after the `#`) that contains a JSON Pointer to a child node. Documents
beyond the first do not need to follow a specific schema.

### Registry

We'll keep in memory a registry of URIs without fragments to JSON nodes. This
registry will also contain a reference to the original document for use with
error reporting.

### Validation

Validation will be performed on the root document. Additional validation may be
done on schema documents based on the dialect of JSON Schema.

### Error reporting

Error entries will contain the URI to the node with the error, a reference to a
resource string, and a dictionary for error parameters.

### Abstractions

Abstractions will be [based on the latest OpenApi
specification][latest-spec-schema]; these interfaces will be implemented per
supported specification (with re-use where available) to work as an adapter
layer to the underlying JSON nodes.

### YAML support

YAML support will be conducted by converting YAML documents to JSON Nodes via
[Yaml2JsonNode][json-everything]. YAML documents with an `$id` field will be
registered in the registry, with the first also receiving the root URI that
loaded the document.

### Schema mapping

Mapping from any URI representing a JSON Schema to its corresponding
platform-specific implementation should be allowed.

- A C# mapping would be a fully qualified type name, such as
  `global:System.DateOnly`.
- A TS mapping would be a file path and export name or an inline data type.

### Extensions

All extensions within the document should also be able to be mapped via an
external options file.

[json-everything]: https://docs.json-everything.net/
[uri-reference]: https://spec.openapis.org/oas/v3.1.0#relative-references-in-uris
[latest-spec-schema]: https://spec.openapis.org/oas/v3.1.0#schema
