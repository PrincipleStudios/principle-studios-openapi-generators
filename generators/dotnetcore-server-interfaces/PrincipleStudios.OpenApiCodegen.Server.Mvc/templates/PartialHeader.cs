namespace PrincipleStudios.OpenApiCodegen.Server.Mvc.templates
{
    public record PartialHeader(
        string? appName,
        string? appDescription,
        string? version,
        string? infoEmail
    );
}