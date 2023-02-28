/*
 * Rock Paper Scissors
 *
 * A sample API that uses enums to play rock-paper-scissors
 *
 * OpenAPI spec version: 1.0.0
 * 
 * Generated by: https://principle.tools
 */
#pragma warning disable

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class OpenApiControllerRegistrationExtensions
    {
        public static IMvcBuilder AddOpenApiRockPaperScissors<TRockPaperScissorsController>(this IServiceCollection services)
            where TRockPaperScissorsController : global::PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp.Enum.RockPaperScissorsControllerBase
        {
            return services.AddControllers();
        }
    }
}