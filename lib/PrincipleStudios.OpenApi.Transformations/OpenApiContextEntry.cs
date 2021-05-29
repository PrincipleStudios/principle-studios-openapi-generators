using Microsoft.OpenApi.Interfaces;

namespace PrincipleStudios.OpenApi.Transformations
{
    [System.Diagnostics.DebuggerDisplay("<{Key}, {Element}>")]
    public class OpenApiContextEntry
    {
        public OpenApiContextEntry(string property)
        {
            this.Key = property;
            this.Element = null;
        }
        public OpenApiContextEntry(string key, IOpenApiElement elementEntry)
        {
            this.Key = key;
            this.Element = elementEntry;
        }
        public OpenApiContextEntry(IOpenApiElement elementEntry)
        {
            this.Element = elementEntry;
            this.Key = null;
        }

        public IOpenApiElement? Element { get; }
        public string? Key { get; }
    }
}