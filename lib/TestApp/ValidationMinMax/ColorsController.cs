namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp.ValidationMinMax;

public class ColorsController : ValidationMinMaxColorsControllerBase
{
    protected override Task<GetColorActionResult> GetColor(long id)
    {
        this.DelegateRequest(id);
        return this.DelegateResponse<GetColorActionResult>();
    }
}
