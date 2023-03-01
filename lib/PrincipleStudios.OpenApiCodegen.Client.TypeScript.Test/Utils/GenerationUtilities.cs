using FluentAssertions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.CodeAnalysis;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript.Utils;

internal static class GenerationUtilities
{
    public static async Task<NodeUtility.ProcessResult> ConvertRequest(this CommonDirectoryFixture commonDirectory, string documentName, string operationName, object parameters, object body, string contentType = "application/json")
    {
        await commonDirectory.PrepareOpenApiDirectory(documentName);

        var result = await commonDirectory.TsNode($@"
            import {{ RequestOpts }} from '@principlestudios/openapi-codegen-typescript';
            import {{ conversion }} from './{documentName}/operations/{operationName}';
            const request: RequestOpts = conversion.request({JsonConvert.SerializeObject(parameters)}, {JsonConvert.SerializeObject(body)}, {JsonConvert.SerializeObject(contentType)});
            console.log(JSON.stringify(request));
        ");
        return result;
    }

    // TODO - add extra headers to this
    public static async Task<NodeUtility.ProcessResult> ConvertResponse(this CommonDirectoryFixture commonDirectory, string documentName, string operationName, int statusCode, Optional<object?> body = default, string contentType = "application/json")
    {
        await commonDirectory.PrepareOpenApiDirectory(documentName);

        var result = await commonDirectory.TsNode($@"
            import {{ ResponseArgs }} from '@principlestudios/openapi-codegen-typescript';
            import {{ conversion, Responses }} from './{documentName}/operations/{operationName}';
            const response: ResponseArgs = {{
                status: {statusCode},
                response: {(body.HasValue ? JsonConvert.SerializeObject(body.Value) : "undefined")},
                getResponseHeader(headerName) {{
                    switch (headerName) {{
                        case 'Content-Type': return {JsonConvert.SerializeObject(contentType)};
                        default: throw new Error('unknown header - TODO, support more');
                    }}
                }}
            }};
            const result: Responses = conversion.response(response);
            console.log(JSON.stringify(result));
        ");
        return result;
    }

    // TODO - add query string parameters to this
    public static JToken AssertRequestSuccess(NodeUtility.ProcessResult result, string method, string path, object body, string contentType = "application/json")
    {
        Assert.Equal(0, result.ExitCode);

        var token = JToken.Parse(result.Output);
        Assert.Equal(method, token["method"]?.ToObject<string>());
        Assert.Equal(path, token["path"]?.ToObject<string>());
        Assert.Equal(contentType, token["headers"]?["Content-Type"]?.ToObject<string>());
        CompareJson(token["body"], body);
        return token;
    }

    public static JToken AssertResponseSuccess(NodeUtility.ProcessResult result, int statusCode, Optional<object?> body = default)
    {
        Assert.Equal(0, result.ExitCode);

        var token = JToken.Parse(result.Output);
        Assert.Equal(statusCode, token["statusCode"]?.ToObject<int>());
        if (body.HasValue) CompareJson(token["data"], body.Value);
        return token;
    }

    public static JToken AssertResponseOtherStatusCode(NodeUtility.ProcessResult result, Optional<object?> body = default)
    {
        Assert.Equal(0, result.ExitCode);

        var token = JToken.Parse(result.Output);
        Assert.Equal("other", token["statusCode"]?.ToObject<string>());
        if (body.HasValue) CompareJson(token["data"], body.Value);
        return token;
    }

    public static void CompareJson(JToken? actual, object? expected)
    {
        actual.Should().BeEquivalentTo(
            Newtonsoft.Json.Linq.JToken.FromObject(expected!)
        );
    }
}
