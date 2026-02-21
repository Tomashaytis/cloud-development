var builder = DistributedApplication.CreateBuilder(args);

// Redis
var redis = builder.AddRedis("cache");

// API service
var apiService = builder.AddProject<Projects.CourseManagement_ApiService>("apiservice")
    .WithReference(redis)
    .WithHttpHealthCheck("/health");

// Client
builder.AddProject<Projects.Client_Wasm>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
