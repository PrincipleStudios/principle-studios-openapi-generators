using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace PrincipleStudios.OpenApi.Transformations
{
    public record OpenApiContext(ImmutableList<OpenApiContextEntry> Entries) : IEnumerable<OpenApiContextEntry>
    {
        public OpenApiContext() : this(ImmutableList<OpenApiContextEntry>.Empty) { }

        public static OpenApiContext From(OpenApiDocument document)
        {
            return new OpenApiContext().Append(document);
        }

        public IEnumerator<OpenApiContextEntry> GetEnumerator() => Entries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Entries.GetEnumerator();
        
        public OpenApiContext Append(string? property, string? key, IOpenApiElement elementEntry)
        {
            return this with { Entries = Entries.Add(new OpenApiContextEntry(property, key, elementEntry)) };
        }
        public OpenApiContext Append(OpenApiDocument elementEntry)
        {
            return this with { Entries = Entries.Add(new OpenApiContextEntry(elementEntry)) };
        }

        public string GetKeyFor(IOpenApiElement element)
        {
            for (var i = Entries.Count - 1; i >= 0; i--)
            {
                if (Entries[i].Element == element)
                    return Entries[i].Key!;
            }

            throw new ArgumentException("Context does not contain the element", nameof(element));
        }
        public string? GetLastKeyFor(IOpenApiElement element)
        {
            if (Entries[Entries.Count - 1] is not { Element: var e, Key: var key, Property: var property } || e != element)
                throw new ArgumentException("Context is not initialized properly", nameof(element));
            return key;
        }
        public void AssertLast(IOpenApiElement element)
        {
            GetLastKeyFor(element);
        }
    }
}