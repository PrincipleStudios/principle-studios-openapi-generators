using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.Transformations;
using PrincipleStudios.OpenApi.TypeScript;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using static PrincipleStudios.OpenApiCodegen.TestUtils.DocumentHelpers;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScriptRxJs
{
    using static OptionsHelpers;

    public class TypeScriptOperationTransformerShould
    {
        [Theory]
        [InlineData("petstore.yaml", "findPets")]
        [InlineData("petstore.yaml", "find pet by id")]
        [InlineData("petstore3.json", "addPet")]
        [InlineData("petstore3.json", "updatePet")]
        [InlineData("petstore3.json", "updatePetWithForm")]
        public void TransformController(string documentName, string operationId)
        {
            var document = GetDocument(documentName);
            var options = LoadOptions();

            OpenApiTransformDiagnostic diagnostic = new();

            var transformer = ConstructTarget(document, options);

            var (operation, context) = (from pathKvp in document.Paths
                                        let pathContext = OpenApiContext.From(document).Append(nameof(document.Paths), pathKvp.Key, pathKvp.Value)
                                        from op in pathKvp.Value.Operations
                                        where op.Value.OperationId == operationId
                                        let opContext = pathContext.Append(nameof(pathKvp.Value.Operations), op.Key.ToString(), op.Value)
                                        select (op.Value, opContext)).FirstOrDefault();

            var result = transformer.TransformOperation(operation, context, diagnostic);

            Snapshot.Match(result.SourceText, $"{nameof(TypeScriptOperationTransformerShould)}.{nameof(TransformController)}.{TypeScriptNaming.ToTitleCaseIdentifier(documentName, options.ReservedIdentifiers())}.{TypeScriptNaming.ToTitleCaseIdentifier(operationId, options.ReservedIdentifiers())}");
        }

        private static TypeScriptOperationTransformer ConstructTarget(OpenApiDocument document, TypeScriptSchemaOptions options)
        {
            var handlebarsFactory = new HandlebarsFactory(OperationHandlebarsTemplateProcess.CreateHandlebars);
            var resolver = new TypeScriptSchemaSourceResolver(options, handlebarsFactory, "");
            return new TypeScriptOperationTransformer(resolver, document, options, "", handlebarsFactory);
        }


    }
}
