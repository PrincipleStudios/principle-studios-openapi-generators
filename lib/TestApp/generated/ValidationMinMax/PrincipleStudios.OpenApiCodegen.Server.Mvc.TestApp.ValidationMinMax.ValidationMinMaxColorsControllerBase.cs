/*
 * Min Max Validation
 *
 * A sample API that demonstrates various min/max validation for both numbers and arrays
 *
 * OpenAPI spec version: 1.0.0
 * 
 * Generated by: https://principle.tools
 */
#nullable enable
#nullable disable warnings
#pragma warning disable

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp.ValidationMinMax
{ 
    public abstract class ValidationMinMaxColorsControllerBase : global::Microsoft.AspNetCore.Mvc.ControllerBase
    {
        
        /// <param name="id">id of the color to retrieve</param>
        [global::Microsoft.AspNetCore.Mvc.HttpGet]
        [global::Microsoft.AspNetCore.Mvc.Route("/validation-min-max/colors")]
        // Adds one or more colors
        [global::Microsoft.AspNetCore.Mvc.ProducesResponseType(200, Type = typeof(string))] // application/json
        public async global::System.Threading.Tasks.Task<global::Microsoft.AspNetCore.Mvc.IActionResult> GetColorTypeSafeEntry(
            [global::Microsoft.AspNetCore.Mvc.FromQuery(Name = "id"), global::System.ComponentModel.DataAnnotations.Required, global::System.ComponentModel.DataAnnotations.Range(1, 10000)] long id
        ) => (await GetColor(id)).ActionResult;

        protected abstract global::System.Threading.Tasks.Task<GetColorActionResult> GetColor(
            long id);

        public readonly struct GetColorActionResult
        {
            public readonly global::Microsoft.AspNetCore.Mvc.IActionResult ActionResult;

            private class HeaderActionResult : global::Microsoft.AspNetCore.Mvc.IActionResult
            {
                private readonly global::Microsoft.AspNetCore.Mvc.IActionResult original;
                private readonly global::System.Collections.Generic.IEnumerable<global::System.Collections.Generic.KeyValuePair<string, string>> headers;

                public HeaderActionResult(global::Microsoft.AspNetCore.Mvc.IActionResult original, global::System.Collections.Generic.IEnumerable<global::System.Collections.Generic.KeyValuePair<string, string>> headers)
                {
                    this.original = original;
                    this.headers = headers;
                }

                public global::System.Threading.Tasks.Task ExecuteResultAsync(global::Microsoft.AspNetCore.Mvc.ActionContext context)
                {
                    foreach (var header in headers)
                        context.HttpContext.Response.Headers[header.Key] = global::Microsoft.Extensions.Primitives.StringValues.Concat(context.HttpContext.Response.Headers[header.Key], header.Value);
                    return original.ExecuteResultAsync(context);
                }
            }

            private GetColorActionResult(global::Microsoft.AspNetCore.Mvc.IActionResult ActionResult, global::System.Collections.Generic.KeyValuePair<string, string>[] headers)
            {
                this.ActionResult = new HeaderActionResult(ActionResult, headers);
            }

            private GetColorActionResult(int statusCode, params global::System.Collections.Generic.KeyValuePair<string, string>[] headers)
                : this(new global::Microsoft.AspNetCore.Mvc.StatusCodeResult(statusCode), headers)
            {
            }

            private GetColorActionResult(int statusCode, string mediaType, global::System.Type declaredType, object? resultObject, params global::System.Collections.Generic.KeyValuePair<string, string>[] headers)
                : this(new global::Microsoft.AspNetCore.Mvc.ObjectResult(resultObject)
                {
                    ContentTypes = new global::Microsoft.AspNetCore.Mvc.Formatters.MediaTypeCollection { { new global::Microsoft.Net.Http.Headers.MediaTypeHeaderValue(mediaType) } },
                    DeclaredType = declaredType,
                    StatusCode = statusCode
                }, headers)
            {
            }
            
            /// <summary>
            /// Adds one or more colors
            /// </summary>
            public static GetColorActionResult Ok(string result) =>
                new GetColorActionResult(200, "application/json", typeof(string), result);
            

            /// <summary>Allows for action results not specified in the API</summary>
            public static GetColorActionResult Unsafe(global::Microsoft.AspNetCore.Mvc.IActionResult actionResult, params global::System.Collections.Generic.KeyValuePair<string, string>[] headers) =>
                new GetColorActionResult(actionResult, headers);
        }
    }
}