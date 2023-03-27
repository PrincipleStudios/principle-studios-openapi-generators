namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp.OAuth;

public class InfoController : InfoControllerBase
{
    protected override Task<GetInfoActionResult> GetInfo()
    {
        this.DelegateRequest();
        return this.DelegateResponse<GetInfoActionResult>();
    }

}

public class AddressController : AddressControllerBase
{
    protected override Task<GetAddressActionResult> GetAddress()
    {
        this.DelegateRequest();
        return this.DelegateResponse<GetAddressActionResult>();
    }
}
