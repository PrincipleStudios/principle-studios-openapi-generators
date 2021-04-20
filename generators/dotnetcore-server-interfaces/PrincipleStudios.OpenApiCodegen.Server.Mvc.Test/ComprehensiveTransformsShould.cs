using PrincipleStudios.OpenApi.CSharp;
using PrincipleStudios.OpenApi.Transformations;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static PrincipleStudios.OpenApiCodegen.Server.Mvc.DocumentHelpers;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc
{
    public class ComprehensiveTransformsShould
    {
        [InlineData(0)]
        [InlineData(1)]
        [Theory]
        public void CoverFullFiles(int index)
        {
            var name = GetDocumentName(index);
            var document = GetDocument(index);

            var schemaTransformer = new CSharpPathControllerTransformer(document, "PS.Controller");
            var transformer = schemaTransformer.ToOpenApiSourceTransformer();

            var entries = transformer.ToSourceEntries(document);
            foreach (var entry in entries)
            {
                Snapshot.Match(entry.SourceText, $"{nameof(ComprehensiveTransformsShould)}.{CSharpNaming.ToTitleCaseIdentifier(name)}.{CSharpNaming.ToTitleCaseIdentifier(entry.Key)}");
            }
        }
    }
}
