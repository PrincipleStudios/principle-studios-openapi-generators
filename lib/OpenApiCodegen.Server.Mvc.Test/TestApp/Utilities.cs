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
	internal static Func<HttpResponseMessage, Task> VerifyResponse(int statusCode)
	{
		return (message) =>
		{
			Assert.Equal(statusCode, (int)message.StatusCode);
			Assert.NotNull(message.Content);
			Assert.Null(message.Content.Headers.ContentType?.MediaType);
			return Task.CompletedTask;
		};
	}

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

	internal static Task TestValidationError<T>(Func<IActionResult, KeyValuePair<string, string>[], T> unsafeFactory, Func<HttpClient, Task<HttpResponseMessage>> performRequest) =>
		TestSingleRequest<T>(new(
			unsafeFactory(new BadRequestResult(), Array.Empty<KeyValuePair<string, string>>()),
			performRequest
		)
		{
			AssertRequest = (controller, request) =>
			{
				Assert.False(controller.ModelState.IsValid);
			},
			AssertResponseMessage = VerifyResponse(400)
		});

	internal static Task TestSingleRequest<T>(MvcRequestTest<T, object?> testDefinition) =>
		TestSingleRequest<T, object?>(testDefinition);

	internal static async Task TestRequestBlockedByFramework(Func<HttpClient, Task<HttpResponseMessage>> performRequest, Func<HttpResponseMessage, Task> assertResponseMessage)
	{
		using var factory = new TestAppFactory();
		using var client = factory.CreateDefaultClient();
		using var responseMessage = await performRequest(client);

		await assertResponseMessage(responseMessage);
	}


	internal static async Task TestSingleRequest<TResponse, TRequest>(MvcRequestTest<TResponse, TRequest> testDefinition)
	{
		var assertionCompleted = 0;
		using var factory = new TestAppFactory();
		factory.OverrideServices += (services) =>
		{
			services.AddSingleton<IProvideArbitraryResponse<TResponse>>(new ProvideArbitraryResponse<TResponse>(testDefinition.Response, (controller) =>
			{
				Interlocked.Increment(ref assertionCompleted);
			}));
			if (testDefinition.AssertRequest != null)
			{
				services.AddSingleton<IHandleArbitraryRequest<TRequest>>(new HandleArbitraryRequest<TRequest>(testDefinition.AssertRequest));
			}
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

internal class MvcRequestTest<TResponse, TRequest>
{
	public MvcRequestTest(TResponse Response, Func<HttpClient, Task<HttpResponseMessage>> PerformRequest)
	{
		this.Response = Response;
		this.PerformRequest = PerformRequest;
	}

	public TResponse Response { get; }
	public Func<HttpClient, Task<HttpResponseMessage>> PerformRequest { get; }
	public Func<HttpResponseMessage, Task>? AssertResponseMessage { get; init; }
	public Action<ControllerBase, TRequest>? AssertRequest { get; init; }

}

internal record ProvideArbitraryResponse<T>(T Response, Action<ControllerBase> ControllerAssertion) : IProvideArbitraryResponse<T>
{
	public void AssertController(ControllerBase controller)
	{
		ControllerAssertion(controller);
	}
}

internal record HandleArbitraryRequest<TRequest>(Action<ControllerBase, TRequest> RequestAssertion) : IHandleArbitraryRequest<TRequest>
{
	public void AssertRequest(ControllerBase controller, TRequest request)
	{
		RequestAssertion(controller, request);
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
