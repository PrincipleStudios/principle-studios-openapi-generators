using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp;

using static Utilities;

public class LegacyOptionalYamlShould
{
    [Fact]
    public Task Handle_mixed_nullable_optional_with_all_provided() =>
        TestSingleRequest<LegacyOptional.ContrivedControllerBase.ContrivedActionResult, LegacyOptional.ContrivedRequest>(new(
            LegacyOptional.ContrivedControllerBase.ContrivedActionResult.Ok(new LegacyOptional.ContrivedResponse(2, 3, 5)),
            client => client.PostAsync("/nullable-vs-optional-legacy/contrived", JsonContent.Create(new { nullableOnly = 2, optionalOnly = 3, optionalOrNullable = 5 }))
        )
        {
            AssertRequest = (controller, request) =>
            {
                Assert.Equal<int?>(2, request.NullableOnly);
                Assert.Equal<int?>(3, request.OptionalOnly);
                Assert.Equal<int?>(5, request.OptionalOrNullable);
            },
            AssertResponseMessage = VerifyResponse(200, new { nullableOnly = 2, optionalOnly = 3, optionalOrNullable = 5 }),
        });

    [Fact]
    public Task Handle_mixed_nullable_optional_with_none_provided() =>
        TestSingleRequest<LegacyOptional.ContrivedControllerBase.ContrivedActionResult, LegacyOptional.ContrivedRequest>(new(
            LegacyOptional.ContrivedControllerBase.ContrivedActionResult.Ok(new LegacyOptional.ContrivedResponse(null, null, null)),
            client => client.PostAsync("/nullable-vs-optional-legacy/contrived", JsonContent.Create(new { nullableOnly = (object?)null, optionalOnly = (object?)null, optionalOrNullable = (object?)null }))
        )
        {
            AssertRequest = (controller, request) =>
            {
                Assert.Null(request.NullableOnly);
                Assert.Null(request.OptionalOnly);
                Assert.Null(request.OptionalOrNullable);
            },
            AssertResponseMessage = VerifyResponse(200, new { nullableOnly = (object?)null }),
        });



}
