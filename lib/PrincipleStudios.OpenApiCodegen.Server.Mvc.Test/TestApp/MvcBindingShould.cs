using FluentAssertions.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using PrincipleStudios.OpenApiCodegen.Json.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp;

using static Utilities;

public class MvcBindingShould
{
    [Fact]
    public Task HandleAllOfResponses() =>
        TestSingleRequest<AllOf.ContactControllerBase.GetContactActionResult>(new(
            AllOf.ContactControllerBase.GetContactActionResult.Ok(new(FirstName: "John", LastName: "Doe", Id: "john-doe-123")),
            client => client.GetAsync("/contact")
        )
        {
            AssertResponseMessage = VerifyResponse(200, new { firstName = "John", lastName = "Doe", id = "john-doe-123" }),
        });

    [Fact]
    public Task DecodeBase64EncodedQueryData() =>
        TestSingleRequest<ControllerExtensions.InformationControllerBase.GetInfoActionResult>(new(
            ControllerExtensions.InformationControllerBase.GetInfoActionResult.Ok("SomeData"),
            client => client.GetAsync("/api/info")
        )
        {
            AssertResponseMessage = VerifyResponse(200, "SomeData"),
        });

}
