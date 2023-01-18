using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp;

public interface IHandleArbitraryRequest<T>
{
    void AssertRequest(ControllerBase controller, T request);
}

public interface IProvideArbitraryResponse<T>
{
    T Response { get; }

    void AssertController(ControllerBase controller);
}

public static class DelegatingRequestExtensions
{
    public static void DelegateRequest<T>(this ControllerBase controller, T request)
    {
        var handleRequest = controller.HttpContext.RequestServices.GetService<IHandleArbitraryRequest<T>>();
        handleRequest?.AssertRequest(controller, request);
    }

    public static Task<T> DelegateResponse<T>(this ControllerBase controller)
    {
        var getResponse = controller.HttpContext.RequestServices.GetRequiredService<IProvideArbitraryResponse<T>>();
        getResponse.AssertController(controller);
        return Task.FromResult(getResponse.Response);
    }
}