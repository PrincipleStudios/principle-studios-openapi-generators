using PrincipleStudios.OpenApiCodegen.Json.Extensions;

namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp.Form;

public class FormBasicController : FormBasicControllerBase
{
    protected override Task<PostBasicFormActionResult> PostBasicForm(string name, string tag, bool hasIdTag)
    {
        this.DelegateRequest((name, tag, hasIdTag));
        return this.DelegateResponse<PostBasicFormActionResult>();
    }
}
