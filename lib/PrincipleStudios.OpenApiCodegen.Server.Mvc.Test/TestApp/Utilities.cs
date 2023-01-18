using FluentAssertions.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.TestHost;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp;

internal class Utilities
{
    internal static Func<HttpResponseMessage, Task> VerifyResponse(int statusCode, object? jsonBody)
    {
        return async (message) =>
        {
            Assert.Equal(statusCode, (int)message.StatusCode);
            Assert.NotNull(message.Content);
            Assert.Equal("application/json", message.Content.Headers.ContentType?.MediaType);
            var actualBody = await message.Content.ReadAsStringAsync();
            CompareJson(actualBody, jsonBody);
        };
    }

    internal static async Task TestSingleRequest<T>(MvcRequestTest<T> testDefinition)
    {
        var assertionCompleted = 0;
        using var factory = new TestAppFactory();
        factory.OverrideServices += (services) =>
        {
            services.AddSingleton<IProvideArbitraryResponse<T>>(new ProvideArbitraryResponse<T>(testDefinition.Response, (controller) =>
            {
                Interlocked.Increment(ref assertionCompleted);
            }));
        };

        using var client = factory.CreateDefaultClient();
        using var responseMessage = await testDefinition.PerformRequest(client);

        if (testDefinition.AssertResponseMessage != null)
            await testDefinition.AssertResponseMessage(responseMessage);

        Assert.Equal(1, assertionCompleted);
    }
    internal static void CompareJson(string actualJson, object? expected)
    {
        Newtonsoft.Json.Linq.JToken.Parse(actualJson).Should().BeEquivalentTo(
            expected == null ? Newtonsoft.Json.Linq.JValue.CreateNull() : Newtonsoft.Json.Linq.JToken.FromObject(expected)
        );
    }
}

internal class MvcRequestTest<T>
{
    public MvcRequestTest(T Response, Func<HttpClient, Task<HttpResponseMessage>> PerformRequest)
    {
        this.Response = Response;
        this.PerformRequest = PerformRequest;
    }

    public T Response { get; }
    public Func<HttpClient, Task<HttpResponseMessage>> PerformRequest { get; }
    public Func<HttpResponseMessage, Task>? AssertResponseMessage { get; init; }

}

internal record ProvideArbitraryResponse<T>(T Response, Action<ControllerBase> ControllerAssertion) : IProvideArbitraryResponse<T>
{
    public void AssertController(ControllerBase controller)
    {
        ControllerAssertion(controller);
    }
}

public class TestAppFactory : WebApplicationFactory<Startup>
{
    public event Action<IServiceCollection>? OverrideServices;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            OverrideServices?.Invoke(services);
        });
    }
}
