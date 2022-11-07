using FluentAssertions.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Client.CSharp;

public class HttpRequestMessageFactoriesShould : IClassFixture<TempDirectory>
{
    private readonly string workingDirectory;

    public HttpRequestMessageFactoriesShould(TempDirectory directory)
    {
        workingDirectory = directory.DirectoryPath;
    }

    [Fact(Skip = "Test fails, see github #85")]
    public async Task PassBase64EncodedQueryData()
    {
        var actualMessage = await GetRequestMessage("controller-extension.yaml", "GetInfo(data: Optional.Create(new byte[] { 1, 2, 3 }))");

        Assert.Equal("GET", actualMessage.Method.Method);
        Assert.Equal("api/info?data=AQI%3D", actualMessage.RequestUri?.OriginalString);
        Assert.Null(actualMessage.Content);
    }

    [Fact]
    public async Task SerializeDictionaries()
    {
        var actualMessage = await GetRequestMessage("dictionary-ref.yaml", @"LookupRecord(lookupRecordBody: new() { [""key1""] = ""value1"", [""key\"":2""] = ""value\n2"" })");

        Assert.Equal("POST", actualMessage.Method.Method);
        Assert.Equal("address", actualMessage.RequestUri?.OriginalString);
        Assert.NotNull(actualMessage.Content);
        var jsonContent = await actualMessage.Content!.ReadAsStringAsync();

        CompareJson(jsonContent, new Dictionary<string, string> { ["key1"] = "value1", ["key\":2"] = "value\n2" });
    }

    [Fact]
    public async Task SerializeEnumerations()
    {
        var actualMessage = await GetRequestMessage("enum.yaml", @"PlayRockPaperScissors(playRockPaperScissorsBody: new(Player1: Option.Rock, Player2: Option.Paper))");

        Assert.Equal("POST", actualMessage.Method.Method);
        Assert.Equal("rock-paper-scissors", actualMessage.RequestUri?.OriginalString);
        Assert.NotNull(actualMessage.Content);
        var jsonContent = await actualMessage.Content!.ReadAsStringAsync();

        CompareJson(jsonContent, new { player1 = "rock", player2 = "paper" });
    }

    [Fact(Skip = "Test fails, see github #86")]
    public async Task PassBase64EncodedHeaderData()
    {
        var actualMessage = await GetRequestMessage("headers.yaml", "GetInfo(xData: Optional.Create(new byte[] { 1, 2, 3 }))");

        Assert.Equal("GET", actualMessage.Method.Method);
        Assert.Equal("info", actualMessage.RequestUri?.OriginalString);
        Assert.Collection(actualMessage.Headers.GetValues("X-Data"),
            value => Assert.Equal("AQI=", value)
        );
        Assert.Null(actualMessage.Content);
    }

    [Fact]
    public async Task HandleInlineObjects()
    {
        var actualMessage = await GetRequestMessage("no-refs.yaml", @"LookupRecord(lookupRecordBody: new(FormattedAddress: ""123 Main St, Richardson, TX 75081"", Location: new (Latitude: 32.9494998, Longitude: -96.7331253)))");

        Assert.Equal("POST", actualMessage.Method.Method);
        Assert.Equal("address", actualMessage.RequestUri?.OriginalString);
        Assert.NotNull(actualMessage.Content);
        var jsonContent = await actualMessage.Content!.ReadAsStringAsync();

        CompareJson(jsonContent, new { formattedAddress = "123 Main St, Richardson, TX 75081", location = new { latitude = 32.9494998, longitude = -96.7331253 } });
    }

    [Fact]
    public async Task HandleOptionalRequestBodyParameters()
    {
        var actualMessage = await GetRequestMessage("nullable-vs-optional.yaml", @"Search(searchBody: new(Name: ""John Smith"", Department: null))");

        Assert.Equal("POST", actualMessage.Method.Method);
        Assert.Equal("search", actualMessage.RequestUri?.OriginalString);
        Assert.NotNull(actualMessage.Content);
        var jsonContent = await actualMessage.Content!.ReadAsStringAsync();

        CompareJson(jsonContent, new { name = "John Smith" });
    }

    [Fact]
    public async Task HandleOptionalRequestBodyParametersThatAreProvided()
    {
        var actualMessage = await GetRequestMessage("nullable-vs-optional.yaml", @"Search(searchBody: new(Name: ""John Smith"", Department: Optional.Create(""Engineering"")))");

        Assert.Equal("POST", actualMessage.Method.Method);
        Assert.Equal("search", actualMessage.RequestUri?.OriginalString);
        Assert.NotNull(actualMessage.Content);
        var jsonContent = await actualMessage.Content!.ReadAsStringAsync();

        CompareJson(jsonContent, new { name = "John Smith", department = "Engineering" });
    }

    [Fact]
    public async Task NotAlterOperationsWhenSecurityIsRequired()
    {
        var actualMessage = await GetRequestMessage("oauth.yaml", @"GetInfo()");

        Assert.Equal("GET", actualMessage.Method.Method);
        Assert.Equal("info", actualMessage.RequestUri?.OriginalString);
    }

    [Fact]
    public async Task AllowQueryInPathWithoutOptionalParameters()
    {
        var actualMessage = await GetRequestMessage("query-in-path.yaml", "TestPath(limit: null)");

        Assert.Equal("GET", actualMessage.Method.Method);
        Assert.Equal("path?param1=test&param2=test&", actualMessage.RequestUri?.OriginalString);
        Assert.Null(actualMessage.Content);
    }

    [Fact]
    public async Task AllowQueryInPathWithOptionalParameters()
    {
        var actualMessage = await GetRequestMessage("query-in-path.yaml", "TestPath(limit: Optional.Create(15))");

        Assert.Equal("GET", actualMessage.Method.Method);
        Assert.Equal("path?param1=test&param2=test&limit=15", actualMessage.RequestUri?.OriginalString);
        Assert.Null(actualMessage.Content);
    }

    private async Task<HttpRequestMessage> GetRequestMessage(string documentName, string operationAndParameters)
    {
        var libBytes = DynamicCompilation.GetGeneratedLibrary(documentName);

        var fullPath = Path.Combine(workingDirectory, Path.GetRandomFileName());
        File.WriteAllBytes(fullPath, libBytes);

        var scriptOptions = ScriptOptions.Default
            .AddReferences(DynamicCompilation.SystemTextCompilationRefPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray())
            .AddReferences(MetadataReference.CreateFromFile(fullPath))
            .WithImports(
                "PS.Controller",
                "PS.Controller.Operations",
                "PrincipleStudios.OpenApiCodegen.Json.Extensions"
            );

        return (HttpRequestMessage)await CSharpScript.EvaluateAsync($"{operationAndParameters}", scriptOptions);

    }

    private void CompareJson(string actualJson, object expected)
    {
        Newtonsoft.Json.Linq.JToken.Parse(actualJson).Should().BeEquivalentTo(
            Newtonsoft.Json.Linq.JToken.FromObject(expected)
        );
    }
}
