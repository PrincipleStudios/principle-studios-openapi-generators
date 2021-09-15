using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using PrincipleStudios.OpenApi.CSharp;
using PrincipleStudios.OpenApi.Transformations;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static PrincipleStudios.OpenApiCodegen.TestUtils.DocumentHelpers;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc
{
    using static OptionsHelpers;

    public class CSharpControllerTransformerShould
    {
        [Theory]
        [InlineData("petstore.yaml", "/pets")]
        [InlineData("petstore.yaml", "/pets/{id}")]
        [InlineData("petstore3.json", "/pet")]
        [InlineData("petstore3.json", "/pet/findByStatus")]
        [InlineData("petstore3.json", "/pet/findByTags")]
        [InlineData("petstore3.json", "/pet/{petId}/uploadImage")]
        [InlineData("petstore3.json", "/store/inventory")]
        [InlineData("petstore3.json", "/store/order")]
        [InlineData("petstore3.json", "/store/order/{orderId}")]
        [InlineData("petstore3.json", "/user")]
        [InlineData("petstore3.json", "/user/createWithArray")]
        [InlineData("petstore3.json", "/user/createWithList")]
        [InlineData("petstore3.json", "/user/login")]
        [InlineData("petstore3.json", "/user/logout")]
        [InlineData("petstore3.json", "/user/{username}")]
        public void TransformController(string documentName, string path)
        {
            var document = GetDocument(documentName);
            var options = LoadOptions();

            var target = ConstructTarget(document, options);
            OpenApiTransformDiagnostic diagnostic = new();

            var transformer = new OperationGroupingSourceTransformer(document.Paths[path], OpenApiContext.From(document).Append(nameof(document.Paths), path, document.Paths[path]), (_, _) => (path, null, null), target);

            var result = transformer.GetSources(diagnostic).Single();

            Snapshot.Match(result.SourceText, $"{nameof(CSharpControllerTransformerShould)}.{nameof(TransformController)}.{CSharpNaming.ToTitleCaseIdentifier(documentName, options.ReservedIdentifiers())}.{CSharpNaming.ToTitleCaseIdentifier(path, options.ReservedIdentifiers())}");
        }

        private static CSharpControllerTransformer ConstructTarget(OpenApiDocument document, CSharpSchemaOptions options, string baseNamespace = "PrincipleStudios.Test")
        {
            var handlebarsFactory = new HandlebarsFactory(ControllerHandlebarsTemplateProcess.CreateHandlebars);
            var resolver = new CSharpSchemaSourceResolver("PS.Controller", options, handlebarsFactory, "");
            return new CSharpControllerTransformer(resolver, document, baseNamespace, options, "", handlebarsFactory);
        }

    }
}
