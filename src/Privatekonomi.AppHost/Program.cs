var builder = DistributedApplication.CreateBuilder(args);

// Add the API project
var api = builder.AddProject<Projects.Privatekonomi_Api>("api");

// Add the Web project and reference the API
var web = builder.AddProject<Projects.Privatekonomi_Web>("web")
    .WithReference(api);

builder.Build().Run();
