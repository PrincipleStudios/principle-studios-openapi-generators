using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp;

using static Utilities;

public class OneOfYamlShould
{
    [Fact]
    public Task Handle_OneOf_without_discriminator() =>
        TestSingleRequest<OneOf.PetControllerBase.GetRandomPetActionResult>(new(
            OneOf.PetControllerBase.GetRandomPetActionResult.Ok(new(Dog: new(true, "Shiba Inu"))),
            client => client.GetAsync("/one-of/pet")
        )
        {
            AssertResponseMessage = VerifyResponse(200, new { bark = true, breed = "Shiba Inu" })
        });

    [Fact]
    public Task Handle_OneOf_with_discriminator() =>
        TestSingleRequest<OneOf.PetControllerBase.AddPetActionResult, OneOf.SpecifiedPet>(new(
            OneOf.PetControllerBase.AddPetActionResult.Ok(),
            client => client.PostAsync("/one-of/pet", JsonContent.Create(new { petType = "dog", bark = true, breed = "Shiba Inu" }))
        )
        {
            AssertRequest = (controller, request) =>
            {
                Assert.True(controller.ModelState.IsValid);
                var dog = Assert.IsType<OneOf.Dog>(request?.Dog);
                Assert.Equal("Shiba Inu", dog?.Breed);
                Assert.Equal(true, dog?.Bark);
            },
            AssertResponseMessage = VerifyResponse(200)
        });

    [Fact]
    public Task Handle_invalid_OneOf_with_discriminator() =>
        TestValidationError(
            OneOf.PetControllerBase.AddPetActionResult.Unsafe,
            client => client.PostAsync("/one-of/pet", JsonContent.Create(new { petType = "dog" }))
        );
}
