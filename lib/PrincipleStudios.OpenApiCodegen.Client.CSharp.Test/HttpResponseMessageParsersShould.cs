using FluentAssertions.Json;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using PrincipleStudios.OpenApiCodegen.Json.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Client.CSharp;

public class HttpResponseMessageParsersShould : IClassFixture<TempDirectory>
{
    private readonly string workingDirectory;

    public HttpResponseMessageParsersShould(TempDirectory directory)
    {
        workingDirectory = directory.DirectoryPath;
    }

    [Fact]
    public async Task DeserializeAllOf()
    {
        var result = await DeserializeResponseMessage("all-of.yaml", @"ParseGetContact", 200, "GetContactReturnType.Ok", new { firstName = "John", lastName = "Doe", id = "john-doe-123" });

        Assert.NotNull(result);

        Assert.Equal("John", result.Body.FirstName);
        Assert.Equal("Doe", result.Body.LastName);
        Assert.Equal("john-doe-123", result.Body.Id);
    }

    [Fact]
    public async Task DeserializeAnyWithObject()
    {
        var result = await DeserializeResponseMessage("any.yaml", @"ParseGetData", 200, "GetDataReturnType.Ok", new { foo = "bar" });

        Assert.NotNull(result);

        var body = Assert.IsAssignableFrom<System.Text.Json.Nodes.JsonObject>((object)result.Body);
        Assert.True(body.TryGetPropertyValue("foo", out var fooNode));
        Assert.Equal("bar", fooNode?.GetValue<string>());
    }

    [Fact]
    public async Task DeserializeAnyWithString()
    {
        var result = await DeserializeResponseMessage("any.yaml", @"ParseGetData", 200, "GetDataReturnType.Ok", "foo");

        Assert.NotNull(result);

        var body = Assert.IsAssignableFrom<System.Text.Json.Nodes.JsonValue>((object)result.Body);
        Assert.Equal("foo", body?.GetValue<string>());
    }

    [Fact]
    public async Task DeserializeStringArray()
    {
        var result = await DeserializeResponseMessage("array.yaml", @"ParseGetColors", 200, "GetColorsReturnType.Ok", new[] { "red", "green", "blue" });

        Assert.NotNull(result);

        var body = Assert.IsAssignableFrom<IEnumerable<string>>((object)result.Body);
        Assert.Collection(body, 
            (elem) => Assert.Equal("red", elem),
            (elem) => Assert.Equal("green", elem),
            (elem) => Assert.Equal("blue", elem));
    }

    [Fact]
    public async Task DeserializeArrayOfStringArray()
    {
        var result = await DeserializeResponseMessage("array.yaml", @"ParseGetPalettes", 200, "GetPalettesReturnType.Ok", new[] { new[] { "red", "green", "blue" }, new[] { "cyan", "magenta", "yellow" } });

        Assert.NotNull(result);

        var body = Assert.IsAssignableFrom<IEnumerable<IEnumerable<string>>>((object)result.Body);
        Assert.Collection(body,
            (palette) => Assert.Collection(palette,
                (elem) => Assert.Equal("red", elem),
                (elem) => Assert.Equal("green", elem),
                (elem) => Assert.Equal("blue", elem)),
            (palette) => Assert.Collection(palette,
                (elem) => Assert.Equal("cyan", elem),
                (elem) => Assert.Equal("magenta", elem),
                (elem) => Assert.Equal("yellow", elem)));
    }

    [Fact]
    public async Task DeserializeDictionary()
    {
        var result = await DeserializeResponseMessage("dictionary-ref.yaml", @"ParseLookupRecord", 200, "LookupRecordReturnType.Ok", new { foo = "bar", boo = "baz" });

        Assert.NotNull(result);

        var body = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>((object)result.Body);
        Assert.Equal(2, body.Count);
        Assert.True(body.ContainsKey("foo"));
        Assert.Equal("bar", body["foo"]);
        Assert.True(body.ContainsKey("boo"));
        Assert.Equal("baz", body["boo"]);
    }

    [Fact]
    public async Task DeserializeEnum()
    {
        var result = await DeserializeResponseMessage("enum.yaml", @"ParsePlayRockPaperScissors", 200, "PlayRockPaperScissorsReturnType.Ok", "player1");

        Assert.NotNull(result);

        Assert.Equal("Player1", (string)result.Body.ToString("g"));
    }

    [Fact]
    public async Task DeserializeConflictStatusCode()
    {
        var result = await DeserializeResponseMessage("enum.yaml", @"ParsePlayRockPaperScissors", 409, "PlayRockPaperScissorsReturnType.Conflict");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task DeserializeResponseHeader()
    {
        var result = await DeserializeResponseMessage("headers.yaml", @"ParseGetInfo", 204, "GetInfoReturnType.NoContent", msg =>
        {
            msg.Headers.Add("X-Data", "custom-header-value");
        });

        Assert.NotNull(result);

        HttpResponseMessage rawMessage = Assert.IsAssignableFrom<HttpResponseMessage>(result.Response);
        Assert.Collection(rawMessage.Headers.GetValues("X-Data"), v => Assert.Equal("custom-header-value", v));

        // TODO - custom headers should probably be able to be accessed via strongly typed props
        // Assert.Equal("custom-header-value", (string)result.XData);
    }

    [Fact]
    public async Task DeserializeDeepObjectCreatedFromNoRefs()
    {
        var addr1 = new { formattedAddress = "123 Main St, Richardson, TX 75081", location = new { latitude = 32.9494998, longitude = -96.7331253 } };
        var addr2 = new { formattedAddress = "123 Main St, Dallas, TX 75252", location = new { latitude = 32.7829188, longitude = -96.7906893 } };
        var result = await DeserializeResponseMessage("no-refs.yaml", @"ParseLookupRecord", 409, "LookupRecordReturnType.Conflict", new
        {
            multiple = new[] {
                addr1,
                addr2,
            }
        });

        Assert.NotNull(result);

        // .Value is needed because Multiple is optional - normally this would use an extension method, but I can't via Optional.
        var resultAddresses = Assert.IsAssignableFrom<IEnumerable<object>>((object)result.Body.Multiple.Value);
        Assert.Collection(resultAddresses, 
            (dynamic entry) => Assert.Equal(addr1.formattedAddress, entry.FormattedAddress), 
            (dynamic entry) => Assert.Equal(addr2.formattedAddress, entry.FormattedAddress)
        );
    }

    [Fact]
    public async Task DeserializeOptionalNullValuesAsPresent()
    {
        var result = await DeserializeResponseMessage("nullable-vs-optional.yaml", @"ParseContrived", 200, "ContrivedReturnType.Ok", new
        {
            nullableOnly = (int?)null,
            optionalOnly = 15,
            optionalOrNullable = (int?)null,
        });

        Assert.NotNull(result);

        Assert.Null((object)result.Body.NullableOnly);
        var optionalOnly = Assert.IsAssignableFrom<Optional<int>>((object)result.Body.OptionalOnly);
        Assert.Equal(15, optionalOnly.GetValueOrDefault());
        var optionalOrNullable = Assert.IsAssignableFrom<Optional<int?>>((object)result.Body.OptionalOrNullable);
        Assert.NotNull(optionalOrNullable);
        Assert.True(optionalOrNullable.TryGet(out var nullable));
        Assert.Null(nullable);
    }

    [Fact]
    public async Task DeserializeOptionalNullValuesAsNotPresent()
    {
        var result = await DeserializeResponseMessage("nullable-vs-optional.yaml", @"ParseContrived", 200, "ContrivedReturnType.Ok", new
        {
            nullableOnly = (int?)null,
        });

        Assert.NotNull(result);

        Assert.Null((object)result.Body.NullableOnly);
        Assert.Null(result.Body.OptionalOnly);
        Assert.Null(result.Body.OptionalOrNullable);
    }

    [Fact]
    public async Task DeserializeSelfReferentialObjects()
    {
        var result = await DeserializeResponseMessage("self-ref.yaml", @"ParseGetData", 200, "GetDataReturnType.Ok", new
        {
            id = 1,
            target = new { id = 2 },
        });

        Assert.NotNull(result);

        Assert.NotNull(result.Body._Target);
        Assert.Equal(1, result.Body.Id);
        Assert.Null(result.Body._Target.Value._Target);
        Assert.Equal(2, result.Body._Target.Value.Id);
    }

    // Using dynamic for this because it is returned from an EvaluateAsync script of a type we don't have - it was generated from the yaml during runtime!
    private Task<dynamic> DeserializeResponseMessage(string documentName, string operation, int statusCode, string parsedType, object? jsonBody, Action<HttpResponseMessage>? additionalMessageSetup = null) =>
        DeserializeResponseMessage(documentName, operation, statusCode, parsedType, responseMessage => {
            responseMessage.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(jsonBody), Encoding.UTF8, "application/json");

            additionalMessageSetup?.Invoke(responseMessage);
        });

    private async Task<dynamic> DeserializeResponseMessage(string documentName, string operation, int statusCode, string parsedType, Action<HttpResponseMessage>? additionalMessageSetup = null)
    {
        using var responseMessage = new HttpResponseMessage((System.Net.HttpStatusCode)statusCode);
        additionalMessageSetup?.Invoke(responseMessage);

        var libBytes = DynamicCompilation.GetGeneratedLibrary(documentName);

        var fullPath = Path.Combine(workingDirectory, Path.GetRandomFileName());
        File.WriteAllBytes(fullPath, libBytes);

        var scriptOptions = ScriptOptions.Default
            .AddReferences(DynamicCompilation.SystemTextCompilationRefPaths.Select(r => Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(r)).ToArray())
            .AddReferences(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(fullPath))
            .WithImports(
                "PS.Controller",
                "PS.Controller.Operations",
                "PrincipleStudios.OpenApiCodegen.Json.Extensions"
            );

        return await CSharpScript.EvaluateAsync($@"
            var parsed = await PS.Controller.Operations.{operation}(Message);

            parsed is {parsedType} ? parsed : null
        ", scriptOptions, globals: new ResponseMessageGlobals(responseMessage));
    }

    public record ResponseMessageGlobals(HttpResponseMessage Message);
}
