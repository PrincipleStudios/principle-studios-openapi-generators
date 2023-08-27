using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PrincipleStudios.OpenApi.Transformations.DocumentTypes;

public interface IDocumentTypeLoader
{
	DocumentTypes.IDocumentReference LoadDocument(Uri retrievalUri, Stream stream);
}
