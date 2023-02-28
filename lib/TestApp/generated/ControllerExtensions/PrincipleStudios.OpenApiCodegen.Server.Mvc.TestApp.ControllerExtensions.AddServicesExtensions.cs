/*
 * OAuth Scopes Sample
 *
 * A sample API that uses oauth scopes
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
        public static IMvcBuilder AddOpenApiOAuthScopesSample<TInformationController>(this IServiceCollection services)
            where TInformationController : global::PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp.ControllerExtensions.InformationControllerBase
        {
            return services.AddControllers();
        }
    }
}