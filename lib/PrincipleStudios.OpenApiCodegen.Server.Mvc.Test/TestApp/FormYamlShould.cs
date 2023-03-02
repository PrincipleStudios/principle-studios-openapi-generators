﻿using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp;

using static Utilities;

public class FormYamlShould
{
    [Fact]
    public Task Handle_FormUrlEncodedContent_requests() =>
        TestSingleRequest<Form.FormBasicControllerBase.PostBasicFormActionResult, (string Name, string Tag, bool HasIdTag)>(new(
            Form.FormBasicControllerBase.PostBasicFormActionResult.Ok(17),
            client => client.PostAsync("/form/basic", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["name"] = "Fido",
                ["tag"] = "dog",
                ["hasIdTag"] = "true",
            }))
        )
        {
            AssertRequest = (controller, request) =>
            {
                Assert.True(controller.ModelState.IsValid);
                Assert.Equal("Fido", request.Name);
                Assert.Equal("dog", request.Tag);
                Assert.Equal(true, request.HasIdTag);
            },
            AssertResponseMessage = VerifyResponse(200, 17),
        });
}