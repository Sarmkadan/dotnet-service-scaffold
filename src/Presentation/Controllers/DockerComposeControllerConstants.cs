namespace DotnetServiceScaffold.Presentation.Controllers;

internal static class DockerComposeControllerConstants
{
    public const string ServiceNameRequired = "ServiceName is required.";
    public const string ImageNameRequired = "ImageName is required.";
    public const string GenerationFailed = "Failed to generate Docker Compose file.";
    public const string YamlContentType = "text/yaml";
    public const string DefaultFileName = "docker-compose.yml";
    public const string GenerateRoute = "generate";
    public const string DownloadRoute = "download";
    public const int SuccessStatusCode = 200;
    public const int BadRequestStatusCode = 400;
    public const int InternalServerErrorStatusCode = 500;
    public const string LogGeneratedMessage = "Generated Docker Compose for service '{ServiceName}'";
    public const string LogErrorGeneratingMessage = "Error generating Docker Compose for '{ServiceName}'";
    public const string LogErrorDownloadingMessage = "Error creating Docker Compose download for '{ServiceName}'";
}
