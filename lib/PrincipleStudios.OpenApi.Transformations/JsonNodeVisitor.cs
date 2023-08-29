using Json.Pointer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace PrincipleStudios.OpenApi.Transformations;

public abstract class JsonNodeVisitor
{
	public virtual void Visit(JsonNode? node, JsonPointer? elementPointer = default)
	{
		elementPointer ??= JsonPointer.Empty;
		switch (node)
		{
			case JsonObject obj:
				VisitObject(obj, elementPointer);
				return;
			case JsonArray array:
				VisitArray(array, elementPointer);
				return;
			case JsonValue value when value.TryGetValue<string>(out var stringValue):
				VisitString(stringValue, elementPointer);
				return;
			case JsonValue value when value.TryGetValue<bool>(out var boolValue):
				VisitBoolean(boolValue, elementPointer);
				return;
			case JsonValue value when value.TryGetValue<decimal>(out var numberValue):
				VisitNumber(value, numberValue, elementPointer);
				return;
			case null:
				VisitNull(elementPointer);
				return;
			default:
				throw new NotSupportedException();
		};
	}

	protected virtual void VisitUndefined(JsonPointer elementPointer) { }

	protected virtual void VisitObject(JsonObject obj, JsonPointer elementPointer)
	{
		foreach (var entry in obj)
		{
			Visit(entry.Value, elementPointer.Combine(PointerSegment.Create(entry.Key)));
		}
	}

	protected virtual void VisitArray(JsonArray array, JsonPointer elementPointer)
	{
		foreach (var entry in array.Select((entry, index) => (entry, index)))
		{
			Visit(entry.entry, elementPointer.Combine(PointerSegment.Create(entry.index.ToString(CultureInfo.InvariantCulture))));
		}
	}

	protected virtual void VisitString(string stringValue, JsonPointer elementPointer) { }

	// JavaScript uses 64-bit floating point numbers (double), but other json serializers may include int64 or larger numbers. Raw Value is provided here.
	protected virtual void VisitNumber(JsonValue value, decimal numberValue, JsonPointer elementPointer) { }

	protected virtual void VisitBoolean(bool booleanValue, JsonPointer elementPointer) { }

	protected virtual void VisitNull(JsonPointer elementPointer) { }
}
