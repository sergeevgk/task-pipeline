# task-pipeline
Test assignment: a Backend web API project for creating and running pipelines of tasks.
The project is build with **.NET 8**, C#, EF, Aspire.

Overview of a web API project:

- There is a list of predefined users in DB. The application requires to pass a token to access some endpoints. For tesing purposes one can use the provided test tokens "token_123", "token_456", "token_789". Use one of those in a special authorize action in Swagger or include in a special header 'UserApiKey' in your requests.

- Create some tasks, create an empty pipeline and observe the collections using standard REST enpoints like HttpGet("/pipelines"), HttpGet("/pipeline/{id:int}"), HttpPost("/pipelines"), HttpGet("/tasks"), HttpGet("/tasks/{id:int}"), HttpPost("/tasks").

- Manage the pipeline items using the endpoints:
	- HttpPost("pipelines/{pipelineId:int}/tasks/{taskId:int}")] to add a task to a pipeline.
	- HttpDelete("pipelines/{pipelineId:int}/tasks/{taskId:int}") to remove a task from a pipeline.

- Check the estimated Pipeline execution time using the endpoint HttpGet("pipelines/{id:int}/time").

- Run the pipeline using the endpoint HttpPost("pipelines/{id:int}/run"). 
	- This actually runs locally a preconfigured external application (see appsettings.json, `ExternalProgramSettings` and `LocalProcessTaskRunManager`). To find out the options for the external application, see `TaskPipeline.ExternalConsoleApp` project.

## Plans
- [ ] Fix the DELETE /pipelines/pipelineId/tasks/taskId endpoint. Currently only allows to remove a first PipelineItem related to the taskId Task. It should be DELETE /pipelines/pipelineId/items/itemId instead.
- [x] Add Cancellation tokens to the pipeline run methods and 'cancel run' endpoint.
- [ ] Handle the concurrent pipeline Start/Stop operations - prevent the pipeline from being run more than once concurrently.
- [ ] Add MongoDB through a repository pattern or using a [preview EF Nuget package for MongoDB](https://www.mongodb.com/docs/entity-framework/current/). Use it instead of current SQLite db.
- [ ] Implement the subsription to the pipeline run results instead of waiting.
- [ ] Add a new type of runnable tasks - using gRPC or Azure Functions.

## Instructions to build and run locally
#### Using Aspire

(0. Make sure that you have .NET Aspire workload installed. Here are [the links of necessary things in one place](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/build-your-first-aspire-app?pivots=visual-studio#prerequisites).)

1. Build and run the project TaskPipeline.AppHost.
2. Follow the link provided in the CLI for Aspire dashboard to access the Swagger for Web API. The dashboard provides convenient logs, metrics and the URLs of the projects it hosts. The ApiService will have its localhost urls included there.

#### Using standard .NET WebAPI approach

1. Run one of the scripts 'publishExternalConsoleApp.bat' or 'publishExternalConsoleApp.sh' depending on your preferred environment. 
    - The script simply publishes a sample console application that plays a role of 'external process' running for every Task in the pipeline. Check the TaskPipeline.ExternalConsoleApp for more details.
2. Build and run the TaskPipeline.ApiService. Follow the link provided by the CLI, go to "/swagger" page for Swagger UI, i.e. "http://localhost:5042/swagger".
