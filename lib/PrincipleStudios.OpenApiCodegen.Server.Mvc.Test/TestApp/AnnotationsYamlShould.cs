using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp;

using static Utilities;

public class AnnotationsYamlShould
{
	[Fact]
	public Task Handle_annotations_response() =>
		TestSingleRequest<Annotations.DogControllerBase.AddDogActionResult, Annotations.Dog>(new(
			Annotations.DogControllerBase.AddDogActionResult.Ok(),
			client => client.PostAsync("/annotations/dog", JsonContent.Create(new { bark = true, breed = "Dachshund", lifeExpectancy = 5 }))
		)
		{
			AssertRequest = (controller, request) =>
			{
				Assert.True(controller.ModelState.IsValid);
				var dog = Assert.IsType<Annotations.Dog>(request);
				Assert.Equal(true, dog?.Bark);
				Assert.Equal("Dachshund", dog?.Breed);
				Assert.Equal(5, dog?.LifeExpectancy);
			},
			AssertResponseMessage = VerifyResponse(200)
		});

	[Fact]
	public Task Handle_annotations_response_invalid_pattern() =>
		TestSingleRequest<Annotations.DogControllerBase.AddDogActionResult, Annotations.Dog>(new(
			Annotations.DogControllerBase.AddDogActionResult.Unsafe(new BadRequestResult()),
			client => client.PostAsync("/annotations/dog", JsonContent.Create(new { bark = true, breed = "012345", lifeExpectancy = 5 }))
		)
		{
			AssertRequest = (controller, request) =>
			{
				Assert.False(controller.ModelState.IsValid);
				Assert.Collection(controller.ModelState,
					(stateEntry) =>
					{
						Assert.Equal("Breed", stateEntry.Key);
						Assert.NotNull(stateEntry.Value);
						Assert.Collection(stateEntry.Value.Errors,
							(error) => Assert.StartsWith("The field Breed must match the regular expression", error.ErrorMessage));
					});
			},
			AssertResponseMessage = VerifyResponse(400)
		});

	[Fact]
	public Task Handle_annotations_response_invalid_minLength() =>
		TestSingleRequest<Annotations.DogControllerBase.AddDogActionResult, Annotations.Dog>(new(
			Annotations.DogControllerBase.AddDogActionResult.Unsafe(new BadRequestResult()),
			client => client.PostAsync("/annotations/dog", JsonContent.Create(new { bark = true, breed = "BR", lifeExpectancy = 5 }))
		)
		{
			AssertRequest = (controller, request) =>
			{
				Assert.False(controller.ModelState.IsValid);
				Assert.Collection(controller.ModelState,
					(stateEntry) =>
					{
						Assert.Equal("Breed", stateEntry.Key);
						Assert.NotNull(stateEntry.Value);
						Assert.Collection(stateEntry.Value.Errors,
							(error) => Assert.StartsWith("The field Breed must be a string or array type with a minimum length", error.ErrorMessage));
					});
			},
			AssertResponseMessage = VerifyResponse(400)
		});

	[Fact]
	public Task Handle_annotations_response_invalid_maxLength() =>
		TestSingleRequest<Annotations.DogControllerBase.AddDogActionResult, Annotations.Dog>(new(
			Annotations.DogControllerBase.AddDogActionResult.Unsafe(new BadRequestResult()),
			client => client.PostAsync("/annotations/dog", JsonContent.Create(new { bark = true, breed = "American Leopard Hound", lifeExpectancy = 5 }))
		)
		{
			AssertRequest = (controller, request) =>
			{
				Assert.False(controller.ModelState.IsValid);
				Assert.Collection(controller.ModelState,
					(stateEntry) =>
					{
						Assert.Equal("Breed", stateEntry.Key);
						Assert.NotNull(stateEntry.Value);
						Assert.Collection(stateEntry.Value.Errors,
							(error) => Assert.StartsWith("The field Breed must be a string or array type with a maximum length", error.ErrorMessage));
					});
			},
			AssertResponseMessage = VerifyResponse(400)
		});

	[Fact]
	public Task Handle_annotations_response_invalid_minimum() =>
		TestSingleRequest<Annotations.DogControllerBase.AddDogActionResult, Annotations.Dog>(new(
			Annotations.DogControllerBase.AddDogActionResult.Unsafe(new BadRequestResult()),
			client => client.PostAsync("/annotations/dog", JsonContent.Create(new { bark = true, breed = "Dachshund", lifeExpectancy = 1 }))
		)
		{
			AssertRequest = (controller, request) =>
			{
				Assert.False(controller.ModelState.IsValid);
				Assert.Collection(controller.ModelState,
					(stateEntry) =>
					{
						Assert.Equal("LifeExpectancy", stateEntry.Key);
						Assert.NotNull(stateEntry.Value);
						Assert.Collection(stateEntry.Value.Errors,
							(error) => Assert.StartsWith("The field LifeExpectancy must be between", error.ErrorMessage));
					});
			},
			AssertResponseMessage = VerifyResponse(400)
		});

	[Fact]
	public Task Handle_annotations_response_invalid_maximum() =>
		TestSingleRequest<Annotations.DogControllerBase.AddDogActionResult, Annotations.Dog>(new(
			Annotations.DogControllerBase.AddDogActionResult.Unsafe(new BadRequestResult()),
			client => client.PostAsync("/annotations/dog", JsonContent.Create(new { bark = true, breed = "Dachshund", lifeExpectancy = 100 }))
		)
		{
			AssertRequest = (controller, request) =>
			{
				Assert.False(controller.ModelState.IsValid);
				Assert.Collection(controller.ModelState,
					(stateEntry) =>
					{
						Assert.Equal("LifeExpectancy", stateEntry.Key);
						Assert.NotNull(stateEntry.Value);
						Assert.Collection(stateEntry.Value.Errors,
							(error) => Assert.StartsWith("The field LifeExpectancy must be between", error.ErrorMessage));
					});
			},
			AssertResponseMessage = VerifyResponse(400)
		});

	[Fact]
	public Task Handle_annotations_response_invalid_multiple() =>
		TestSingleRequest<Annotations.DogControllerBase.AddDogActionResult, Annotations.Dog>(new(
			Annotations.DogControllerBase.AddDogActionResult.Unsafe(new BadRequestResult()),
			client => client.PostAsync("/annotations/dog", JsonContent.Create(new { bark = true, breed = "012345", lifeExpectancy = 100 }))
		)
		{
			AssertRequest = (controller, request) =>
			{
				Assert.False(controller.ModelState.IsValid);
				Assert.Contains("Breed", controller.ModelState.Keys);
				Assert.Contains("LifeExpectancy", controller.ModelState.Keys);
				Assert.Equal(2, controller.ModelState.ErrorCount);
			},
			AssertResponseMessage = VerifyResponse(400)
		});

}