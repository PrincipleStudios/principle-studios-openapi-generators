namespace PrincipleStudios.OpenApi.CSharp.Templates
{
    public record PartialHeader(
        string? appName,
        string? appDescription,
        string? version,
        string? infoEmail,
        string codeGeneratorVersionInfo
    );
}