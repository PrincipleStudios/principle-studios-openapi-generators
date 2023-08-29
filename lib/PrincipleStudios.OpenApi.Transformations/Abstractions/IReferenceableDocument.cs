using Json.Pointer;
using System;

namespace PrincipleStudios.OpenApi.Transformations.Abstractions;

public interface IReferenceableDocument
{
	public Uri Id { get; }
}
