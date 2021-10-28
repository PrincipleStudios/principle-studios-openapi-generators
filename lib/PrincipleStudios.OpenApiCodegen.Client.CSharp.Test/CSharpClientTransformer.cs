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

    public class CSharpClientTransformer
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

            var result = target.TransformOperations(diagnostic);

            Snapshot.Match(result.SourceText, $"{nameof(Mvc.CSharpClientTransformer)}.{nameof(TransformController)}.{CSharpNaming.ToTitleCaseIdentifier(documentName, options.ReservedIdentifiers())}.{CSharpNaming.ToTitleCaseIdentifier(path, options.ReservedIdentifiers())}");
        }

        private static OpenApi.CSharp.CSharpClientTransformer ConstructTarget(OpenApiDocument document, CSharpSchemaOptions options, string baseNamespace = "PrincipleStudios.Test")
        {
            var handlebarsFactory = new HandlebarsFactory(ControllerHandlebarsTemplateProcess.CreateHandlebars);
            var resolver = new CSharpSchemaSourceResolver("PS.Controller", options, handlebarsFactory, "");
            return new OpenApi.CSharp.CSharpClientTransformer(resolver, document, baseNamespace, options, "", handlebarsFactory);
        }

    }
}
