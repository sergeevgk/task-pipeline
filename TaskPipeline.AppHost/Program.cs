var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.TaskPipeline_ApiService>("apiservice");

builder.Build().Run();
