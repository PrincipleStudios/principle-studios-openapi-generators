using Json.More;
using Json.Pointer;
using Json.Schema;
using PrincipleStudios.OpenApi.Transformations.Diagnostics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace PrincipleStudios.OpenApi.Transformations.DocumentTypes;

public static class SubschemaLoader
{
	public static JsonSchema? FindSubschema(NodeMetadata nodeInfo)
	{
		var data = Encoding.UTF8.GetBytes(nodeInfo.Node?.ToJsonString() ?? "null");
		var reader = new Utf8JsonReader(data);
		var uriByBytes = GetJsonPointerDictionary(ref reader, nodeInfo.Id);
		try
		{

			return JsonSerializer.Deserialize<JsonSchema>(data, new JsonSerializerOptions
			{
				Converters =
				{
					new JsonSchemaWithIdConverter(uriByBytes),
					new ItemsKeywordJsonConverter(),
					new PropertiesKeywordJsonConverter(),
					new AllOfKeywordJsonConverter(),
					new OneOfKeywordJsonConverter(),
				}
			});
		}
		catch (JsonException ex)
		{
			var uri = uriByBytes[(ex.BytePositionInLine - 1) ?? 0];
			// TODO - use `uri` to record exact location
			throw new DiagnosticException(UnableToParseSchema.Builder(ex));
		}
	}

	private static Dictionary<long, Uri> GetJsonPointerDictionary(ref Utf8JsonReader reader, Uri baseUri)
	{
		var builder = new UriBuilder(baseUri);
		var result = new Dictionary<long, Uri>();
		var current = JsonPointer.Empty;
		while (reader.Read())
		{
			RecordCurrentPosition(ref reader);
			ReadToken(ref reader);
		}
		return result;

		void ReadToken(ref Utf8JsonReader reader)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.StartObject:
					ReadObject(ref reader);
					break;
				case JsonTokenType.StartArray:
					ReadArray(ref reader);
					break;
			}
		}

		void ReadObject(ref Utf8JsonReader reader)
		{
			var prev = current;
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
					case JsonTokenType.PropertyName:
						current = prev.Combine(PointerSegment.Parse(reader.GetString()));
						break;
					case JsonTokenType.EndObject:
						current = prev;
						break;
					default:
						RecordCurrentPosition(ref reader);
						ReadToken(ref reader);
						break;
				}
			}
		}

		void ReadArray(ref Utf8JsonReader reader)
		{
			var prev = current;
			int index = 0;
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
					case JsonTokenType.EndArray:
						current = prev;
						break;
					default:
						current = prev.Combine(PointerSegment.Create((index++).ToString()));
						RecordCurrentPosition(ref reader);
						ReadToken(ref reader);
						break;
				}
			}
		}

		void RecordCurrentPosition(ref Utf8JsonReader reader)
		{
			builder.Fragment = baseUri.Fragment + current.ToString();
			result.Add(reader.TokenStartIndex, builder.Uri);
		}
	}

	private class JsonSchemaWithIdConverter : JsonConverter<JsonSchema>
	{
		private static readonly JsonConverter<JsonSchema> original =
			(Activator.CreateInstance(typeof(JsonSchema).GetCustomAttribute<JsonConverterAttribute>().ConverterType) as JsonConverter<JsonSchema>)!;
		private readonly IReadOnlyDictionary<long, Uri> uriByBytes;

		public JsonSchemaWithIdConverter(IReadOnlyDictionary<long, Uri> uriByBytes)
		{
			this.uriByBytes = uriByBytes;
		}

		public override JsonSchema? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var start = reader.TokenStartIndex;
			if (!uriByBytes.ContainsKey(start))
			{
				// This is probably because someone incorrectly called `JsonSerialiazer.Deserialize` instead of asking for a converter from options.
				throw new InvalidOperationException(Errors.UnsupportedDeserialization);
			}
			var result = InnerRead(ref reader, typeToConvert, options);
			if (result != null)
				result.BaseUri = uriByBytes[start];
			return result;
		}

		private static JsonSchema? InnerRead(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{

			if (reader.TokenType == JsonTokenType.True)
			{
				return JsonSchema.True;
			}

			if (reader.TokenType == JsonTokenType.False)
			{
				return JsonSchema.False;
			}

			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new JsonException("JSON Schema must be true, false, or an object");
			}

			if (!reader.Read())
			{
				throw new JsonException("Expected token");
			}

			var builder = new List<IJsonSchemaKeyword>();
			do
			{
				switch (reader.TokenType)
				{
					case JsonTokenType.PropertyName:
						{
							string keyword = reader.GetString()!;
							reader.Read();
							Type? implementationType = SchemaKeywordRegistry.GetImplementationType(keyword);
							if (implementationType == null)
							{
								var converter = (JsonConverter<JsonNode>)options.GetConverter(typeof(JsonNode));
								JsonNode? value = converter.Read(ref reader, typeof(JsonNode), options);
								UnrecognizedKeyword item = new UnrecognizedKeyword(keyword, value);
								builder.Add(item);
							}
							else
							{
								builder.Add(ReadSchemaKeyword(ref reader, implementationType, options, keyword));
							}

							break;
						}
					case JsonTokenType.EndObject:
						var ctor = typeof(JsonSchema).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(IEnumerable<IJsonSchemaKeyword>) }, null);
						return (JsonSchema)ctor.Invoke(new[] { builder });
					default:
						throw new JsonException("Expected keyword or end of schema object");
					case JsonTokenType.Comment:
						break;
				}
			}
			while (reader.Read());
			throw new JsonException("Expected token");
		}

		private static IJsonSchemaKeyword ReadSchemaKeyword(ref Utf8JsonReader reader, Type implementationType, JsonSerializerOptions options, string keyword)
		{
			var keywordDeserializer = (IKeywordDeserializer)Activator.CreateInstance(typeof(KeywordDeserializer<>).MakeGenericType(implementationType));
			//if (reader.TokenType == JsonTokenType.Null)
			//{
			//	return SchemaKeywordRegistry.GetNullValuedKeyword(implementationType) ?? throw new InvalidOperationException("No null instance registered for keyword `" + keyword + "`");
			//}
			return keywordDeserializer.ReadSchemaKeyword(ref reader, options, keyword);
		}

		public override void Write(Utf8JsonWriter writer, JsonSchema value, JsonSerializerOptions options)
		{
			original.Write(writer, value, options);
		}
	}

	private interface IKeywordDeserializer
	{
		IJsonSchemaKeyword ReadSchemaKeyword(ref Utf8JsonReader reader, JsonSerializerOptions options, string keyword);
	}

#pragma warning disable CA1812
	private class KeywordDeserializer<T> : IKeywordDeserializer where T : IJsonSchemaKeyword
	{
		public IJsonSchemaKeyword ReadSchemaKeyword(ref Utf8JsonReader reader, JsonSerializerOptions options, string keyword)
		{
			var converter = (JsonConverter<T>)options.GetConverter(typeof(T));

			return converter.Read(ref reader, typeof(T), options) ?? throw new InvalidOperationException("Could not deserialize expected keyword `" + keyword + "`");
		}
	}
#pragma warning restore CA1812

	internal class ItemsKeywordJsonConverter : JsonConverter<ItemsKeyword>
	{
		public override ItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.StartArray)
			{
				return new ItemsKeyword(options.Read<List<JsonSchema>>(ref reader)!);
			}
			return new ItemsKeyword(options.Read<JsonSchema>(ref reader)!);
		}

		public override void Write(Utf8JsonWriter writer, ItemsKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName("items");
			if (value.SingleSchema != null)
			{
				JsonSerializer.Serialize(writer, value.SingleSchema, options);
				return;
			}
			writer.WriteStartArray();
			foreach (JsonSchema item in value.ArraySchemas!)
			{
				JsonSerializer.Serialize(writer, item, options);
			}
			writer.WriteEndArray();
		}
	}

	internal class PropertiesKeywordJsonConverter : JsonConverter<PropertiesKeyword>
	{
		public override PropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new JsonException("Expected object");
			}
			return new PropertiesKeyword(options.Read<Dictionary<string, JsonSchema>>(ref reader)!);
		}

		public override void Write(Utf8JsonWriter writer, PropertiesKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName("properties");
			writer.WriteStartObject();
			foreach (KeyValuePair<string, JsonSchema> property in value.Properties)
			{
				writer.WritePropertyName(property.Key);
				JsonSerializer.Serialize(writer, property.Value, options);
			}
			writer.WriteEndObject();
		}
	}

	internal class AllOfKeywordJsonConverter : JsonConverter<AllOfKeyword>
	{
		public override AllOfKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.StartArray)
			{
				return new AllOfKeyword(options.Read<List<JsonSchema>>(ref reader)!);
			}
			JsonSchema jsonSchema = options.Read<JsonSchema>(ref reader)!;
			return new AllOfKeyword(jsonSchema);
		}

		public override void Write(Utf8JsonWriter writer, AllOfKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName("allOf");
			writer.WriteStartArray();
			foreach (JsonSchema schema in value.Schemas)
			{
				JsonSerializer.Serialize(writer, schema, options);
			}
			writer.WriteEndArray();
		}
	}


	internal class OneOfKeywordJsonConverter : JsonConverter<OneOfKeyword>
	{
		public override OneOfKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.StartArray)
			{
				return new OneOfKeyword(options.Read<List<JsonSchema>>(ref reader)!);
			}
			JsonSchema jsonSchema = options.Read<JsonSchema>(ref reader)!;
			return new OneOfKeyword(jsonSchema);
		}

		public override void Write(Utf8JsonWriter writer, OneOfKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName("oneOf");
			writer.WriteStartArray();
			foreach (JsonSchema schema in value.Schemas)
			{
				JsonSerializer.Serialize(writer, schema, options);
			}
			writer.WriteEndArray();
		}
	}
}

public record UnableToParseSchema(JsonException JsonException, Location Location) : DiagnosticBase(Location)
{
	public static DiagnosticException.ToDiagnostic Builder(JsonException JsonException) => (Location) => new UnableToParseSchema(JsonException, Location);
}

public static class SystemTextJsonExtensions
{
	public static T? Read<T>(this JsonSerializerOptions options, ref Utf8JsonReader reader)
	{
		var converter = (JsonConverter<T>)options.GetConverter(typeof(T));
		return converter.Read(ref reader, typeof(T), options);
	}
}