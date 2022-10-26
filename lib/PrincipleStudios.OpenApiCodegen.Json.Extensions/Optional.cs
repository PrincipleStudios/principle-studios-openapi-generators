using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PrincipleStudios.OpenApiCodegen.Json.Extensions;

[JsonConverter(typeof(OptionalJsonConverterFactory))]
public abstract record Optional<T>
{
    private Optional() { }

    public sealed record Present(T Value) : Optional<T>;

    public class Serializer : JsonConverter<Optional<T>>
    {
        public override Optional<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = JsonSerializer.Deserialize<T>(ref reader, options)!;
            return new Present(result);
        }

        public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
        {
            if (value is Optional<T>.Present { Value: var v })
            {
                JsonSerializer.Serialize<T>(writer, v, options);
            }
            else
            {
                // This shouldn't happen! Optional should either be Present or be omitted via property serialization attributes
                throw new InvalidOperationException("Optional should either be Present or omitted via property serialization attributes: [global::System.Text.Json.Serialization.JsonIgnore(Condition = global::System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]");
            }
        }

        public override bool HandleNull => true;
    }
}

public class OptionalJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var genericArg = typeToConvert.GetGenericArguments()[0];

        return (JsonConverter)Activator.CreateInstance(
                typeof(Optional<>.Serializer).MakeGenericType(genericArg),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: Array.Empty<object>(),
                culture: null)!;
    }
}
