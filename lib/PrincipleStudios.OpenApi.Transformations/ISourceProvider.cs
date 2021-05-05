using System.Collections.Generic;

namespace PrincipleStudios.OpenApi.Transformations
{
    public interface ISourceProvider
    {
        IEnumerable<SourceEntry> GetSources();
    }
}