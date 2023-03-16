using Microsoft.AspNetCore.Mvc;
using PrincipleStudios.OpenApiCodegen.Json.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp;

using static Utilities;

public class OptionalYamlShould
{
    [Fact]
    public Task Handle_mixed_nullable_optional_with_all_provided() =>
        TestSingleRequest<NullableVsOptional.ContrivedControllerBase.ContrivedActionResult, NullableVsOptional.ContrivedRequest>(new(
            NullableVsOptional.ContrivedControllerBase.ContrivedActionResult.Ok(new NullableVsOptional.ContrivedResponse(2, Optional.Create(3), Optional.Create<int?>(5))),
            client => client.PostAsync("/nullable-vs-optional/contrived", JsonContent.Create(new { nullableOnly = 2, optionalOnly = 3, optionalOrNullable = 5 }))
        )
        {
            AssertRequest = (controller, request) =>
            {
                Assert.True(controller.ModelState.IsValid);
                Assert.Equal<int?>(2, request.NullableOnly);
                Assert.Equal<Optional<int>?>(Optional.Create(3), request.OptionalOnly);
                Assert.Equal<Optional<int?>?>(Optional.Create<int?>(5), request.OptionalOrNullable);
            },
            AssertResponseMessage = VerifyResponse(200, new { nullableOnly = 2, optionalOnly = 3, optionalOrNullable = 5 }),
        });

    [Fact]
    public Task Disallow_null_values_on_nonnullable_required_properties() =>
        TestSingleRequest<NullableVsOptional.ContrivedControllerBase.ContrivedActionResult, NullableVsOptional.ContrivedRequest>(new(
            NullableVsOptional.ContrivedControllerBase.ContrivedActionResult.Unsafe(new BadRequestResult()),
            client => client.PostAsync("/nullable-vs-optional/contrived", JsonContent.Create(new { nullableOnly = (object?)null, optionalOnly = (object?)null, optionalOrNullable = (object?)null }))
        )
        {
            AssertRequest = (controller, request) =>
            {
                Assert.False(controller.ModelState.IsValid);
            },
            AssertResponseMessage = VerifyResponse(400),
        });

    [Fact]
    public Task Handle_mixed_nullable_optional_with_values_as_intended_by_yaml() =>
        TestSingleRequest<NullableVsOptional.ContrivedControllerBase.ContrivedActionResult, NullableVsOptional.ContrivedRequest>(new(
            NullableVsOptional.ContrivedControllerBase.ContrivedActionResult.Ok(new NullableVsOptional.ContrivedResponse(null, null, null)),
            client => client.PostAsync("/nullable-vs-optional/contrived", JsonContent.Create(new { nullableOnly = (object?)null }))
        )
        {
            AssertRequest = (controller, request) =>
            {
                Assert.True(controller.ModelState.IsValid);
                Assert.Null(request.NullableOnly);
                Assert.Null(request.OptionalOnly);
                Assert.Null(request.OptionalOrNullable);
            },
            AssertResponseMessage = VerifyResponse(200, new { nullableOnly = (object?)null }),
        });

    [Fact]
    public Task Disallow_missing_required_values() =>
        TestSingleRequest<NullableVsOptional.ContrivedControllerBase.ContrivedActionResult, NullableVsOptional.ContrivedRequest>(new(
            NullableVsOptional.ContrivedControllerBase.ContrivedActionResult.Unsafe(new BadRequestResult()),
            client => client.PostAsync("/nullable-vs-optional/contrived", JsonContent.Create(new { }))
        )
        {
            AssertRequest = (controller, request) =>
            {
                Assert.False(controller.ModelState.IsValid);
            },
            AssertResponseMessage = VerifyResponse(400),
        });

}
