var builder = DistributedApplication.CreateBuilder(args);

var publishTestConsoleApp = builder.AddExecutable("publish-ExternalConsoleApp-locally", "dotnet", @"..\", 
	["publish", 
	"TaskPipeline.ExternalConsoleApp\\TaskPipeline.ExternalConsoleApp.csproj",
	"/p:PublishProfile=TaskPipeline.ExternalConsoleApp\\Properties\\PublishProfiles\\FolderProfile.pubxml"]);

builder.AddProject<Projects.TaskPipeline_ApiService>("apiservice");

builder.Build().Run();
