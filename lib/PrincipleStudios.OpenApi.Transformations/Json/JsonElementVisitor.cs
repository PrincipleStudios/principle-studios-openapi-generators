using Json.Pointer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace PrincipleStudios.OpenApi.Transformations.Json;

public abstract class JsonElementVisitor
{
	public virtual void Visit(JsonElement element, JsonPointer? elementPointer = default)
	{
		elementPointer ??= JsonPointer.Empty;
		switch (element.ValueKind)
		{
			case JsonValueKind.Undefined:
				VisitUndefined(elementPointer);
				return;
			case JsonValueKind.Object:
				VisitObject(element, elementPointer);
				return;
			case JsonValueKind.Array:
				VisitArray(element, elementPointer);
				return;
			case JsonValueKind.String:
				VisitString(element, elementPointer);
				return;
			case JsonValueKind.Number:
				VisitNumber(element, elementPointer);
				return;
			case JsonValueKind.True:
				VisitTrue(element, elementPointer);
				return;
			case JsonValueKind.False:
				VisitFalse(element, elementPointer);
				return;
			case JsonValueKind.Null:
				VisitNull(elementPointer);
				return;
			default:
				throw new NotSupportedException();
		};
	}

	protected virtual void VisitUndefined(JsonPointer elementPointer) { }

	protected virtual void VisitObject(JsonElement element, JsonPointer elementPointer)
	{
		foreach (var entry in element.EnumerateObject())
		{
			Visit(entry.Value, elementPointer.Combine(PointerSegment.Create(entry.Name)));
		}
	}

	protected virtual void VisitArray(JsonElement element, JsonPointer elementPointer)
	{
		foreach (var entry in element.EnumerateArray().Select((element, index) => (element, index)))
		{
			Visit(entry.element, elementPointer.Combine(PointerSegment.Create(entry.index.ToString(CultureInfo.InvariantCulture))));
		}
	}

	protected virtual void VisitString(JsonElement element, JsonPointer elementPointer) { }

	protected virtual void VisitNumber(JsonElement element, JsonPointer elementPointer) { }

	protected virtual void VisitTrue(JsonElement element, JsonPointer elementPointer) { }

	protected virtual void VisitFalse(JsonElement element, JsonPointer elementPointer) { }

	protected virtual void VisitNull(JsonPointer elementPointer) { }
}
