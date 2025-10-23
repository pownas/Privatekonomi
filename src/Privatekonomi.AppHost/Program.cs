var builder = DistributedApplication.CreateBuilder(args);

// Add the API project
var api = builder.AddProject<Projects.Privatekonomi_Api>("api")
    .WithExternalHttpEndpoints();

// Add the Web project and reference the API
var web = builder.AddProject<Projects.Privatekonomi_Web>("web")
    .WithReference(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();
