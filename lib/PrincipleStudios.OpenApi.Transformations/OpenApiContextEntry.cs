using Microsoft.OpenApi.Interfaces;

namespace PrincipleStudios.OpenApi.Transformations
{
	[System.Diagnostics.DebuggerDisplay("{Property}[{Key}], {Element}")]
	public class OpenApiContextEntry
	{
		public OpenApiContextEntry(string? property, string? key, IOpenApiElement elementEntry)
		{
			Property = property;
			this.Key = key;
			this.Element = elementEntry;
		}
		public OpenApiContextEntry(IOpenApiElement elementEntry)
		{
			this.Element = elementEntry;
			this.Property = null;
			this.Key = null;
		}

		public IOpenApiElement Element { get; }
		public string? Property { get; }
		public string? Key { get; }
	}
}