namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp.NullableVsOptional;

public class ContrivedController : ContrivedControllerBase
{
    protected override Task<ContrivedActionResult> Contrived(ContrivedRequest contrivedBody)
    {
        this.DelegateRequest(contrivedBody);
        return this.DelegateResponse<ContrivedActionResult>();
    }
}

public class SearchController : SearchControllerBase
{
    protected override Task<SearchActionResult> Search(SearchRequest searchBody)
    {
        this.DelegateRequest(searchBody);
        return this.DelegateResponse<SearchActionResult>();
    }
}
