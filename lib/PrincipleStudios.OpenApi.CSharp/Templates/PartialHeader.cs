namespace PrincipleStudios.OpenApi.CSharp.Templates
{
    public record PartialHeader(
        string? AppName,
        string? AppDescription,
        string? Version,
        string? InfoEmail,
        string CodeGeneratorVersionInfo
    );
}