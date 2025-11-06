var builder = DistributedApplication.CreateBuilder(args);

// Konfigurera för Raspberry Pi - lyssna på alla nätverksinterfaces
if (Environment.GetEnvironmentVariable("PRIVATEKONOMI_RASPBERRY_PI") == "true")
{
    builder.Services.Configure<Microsoft.AspNetCore.Hosting.HostingOptions>(options =>
    {
        // Sätt ingen timeout för Raspberry Pi
        options.ShutdownTimeout = TimeSpan.FromSeconds(30);
    });
    
    builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
    {
        // Lyssna på alla IP-adresser för Raspberry Pi
        options.ListenAnyIP(17127);
    });
}

var aspnetEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    ?? builder.Environment.EnvironmentName;

var dotnetEnvironment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
    ?? aspnetEnvironment;

var privatekonomiEnvironment = Environment.GetEnvironmentVariable("PRIVATEKONOMI_ENVIRONMENT")
    ?? aspnetEnvironment;

var storageProvider = Environment.GetEnvironmentVariable("PRIVATEKONOMI_STORAGE_PROVIDER") ?? "Unknown";

// Add the API project
var api = builder.AddProject<Projects.Privatekonomi_Api>("api")
    .WithExternalHttpEndpoints()
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", aspnetEnvironment)
    .WithEnvironment("DOTNET_ENVIRONMENT", dotnetEnvironment)
    .WithEnvironment("PRIVATEKONOMI_ENVIRONMENT", privatekonomiEnvironment)
    .WithEnvironment("PRIVATEKONOMI_STORAGE_PROVIDER", storageProvider);

// Add the Web project and reference the API
var web = builder.AddProject<Projects.Privatekonomi_Web>("web")
    .WithReference(api)
    .WithExternalHttpEndpoints()
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", aspnetEnvironment)
    .WithEnvironment("DOTNET_ENVIRONMENT", dotnetEnvironment)
    .WithEnvironment("PRIVATEKONOMI_ENVIRONMENT", privatekonomiEnvironment)
    .WithEnvironment("PRIVATEKONOMI_STORAGE_PROVIDER", storageProvider);

builder.Build().Run();
