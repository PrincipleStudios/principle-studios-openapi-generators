using System;
using System.Collections.Generic;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations
{
    public interface IOpenApiDocumentSourceGenerator : IOpenApiDocumentVisitor, ISourceProvider
    {
    }
}
