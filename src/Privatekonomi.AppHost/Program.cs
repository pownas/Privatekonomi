var builder = DistributedApplication.CreateBuilder(args);

var aspnetEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    ?? builder.Environment.EnvironmentName;

var dotnetEnvironment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
    ?? aspnetEnvironment;

var privatekonomiEnvironment = Environment.GetEnvironmentVariable("PRIVATEKONOMI_ENVIRONMENT")
    ?? aspnetEnvironment;

var storageProvider = Environment.GetEnvironmentVariable("PRIVATEKONOMI_STORAGE_PROVIDER") ?? "Unknown";

// Raspberry Pi configuration - listen on all network interfaces
var isRaspberryPi = Environment.GetEnvironmentVariable("PRIVATEKONOMI_RASPBERRY_PI") == "true";
var webUrls = isRaspberryPi ? "http://0.0.0.0:5274" : null;
var apiUrls = isRaspberryPi ? "http://0.0.0.0:5277" : null;

// Add the API project
var apiBuilder = builder.AddProject<Projects.Privatekonomi_Api>("api")
    .WithExternalHttpEndpoints()
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", aspnetEnvironment)
    .WithEnvironment("DOTNET_ENVIRONMENT", dotnetEnvironment)
    .WithEnvironment("PRIVATEKONOMI_ENVIRONMENT", privatekonomiEnvironment)
    .WithEnvironment("PRIVATEKONOMI_STORAGE_PROVIDER", storageProvider);

if (apiUrls != null)
{
    apiBuilder = apiBuilder.WithEnvironment("ASPNETCORE_URLS", apiUrls);
}

var api = apiBuilder;

// Add the Web project and reference the API
var webBuilder = builder.AddProject<Projects.Privatekonomi_Web>("web")
    .WithReference(api)
    .WithExternalHttpEndpoints()
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", aspnetEnvironment)
    .WithEnvironment("DOTNET_ENVIRONMENT", dotnetEnvironment)
    .WithEnvironment("PRIVATEKONOMI_ENVIRONMENT", privatekonomiEnvironment)
    .WithEnvironment("PRIVATEKONOMI_STORAGE_PROVIDER", storageProvider);

if (webUrls != null)
{
    webBuilder = webBuilder.WithEnvironment("ASPNETCORE_URLS", webUrls);
}

var web = webBuilder;

builder.Build().Run();
