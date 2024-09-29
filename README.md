# task-pipeline
Test assignment: a Backend web API project for creating and running pipelines of tasks.
The project is build with **.NET 8**, C#, EF, Aspire.

## Plans
-[ ] Add Cancellation tokens to the pipeline run methods and 'cancel run' endpoint.\
-[ ] Add MongoDB through a repository pattern or using a [preview EF Nuget package for MongoDB](https://www.mongodb.com/docs/entity-framework/current/). Use it instead of current SQLite db.\
-[ ] Implement the subsription to the pipeline run results instead of waiting.\
-[ ] Add a new type of runnable tasks - using gRPC or Azure Functions.

## Instructions to build and run locally
#### Using Aspire

(0. Make sure that you have .NET Aspire workload installed. Here are [the links of necessary things in one place](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/build-your-first-aspire-app?pivots=visual-studio#prerequisites).)

1. Build and run the project TaskPipeline.AppHost.
2. Follow the link provided in the CLI for Aspire dashboard to access the Swagger for Web API. The dashboard provides convenient logs, metrics and the URLs of the projects it hosts. The ApiService will have its localhost urls included there.

#### Using standard .NET WebAPI approach

1. Run one of the scripts 'publishExternalConsoleApp.bat' or 'publishExternalConsoleApp.sh' depending on your preferred environment. The script simply publishes a sample console application that plays a role of 'external process' running for every Task in the pipeline. Check the TaskPipeline.ExternalConsoleApp for more details.
2. Build and run the TaskPipeline.ApiService. Follow the link provided by the CLI, go to "/swagger" page for Swagger UI, i.e. "http://localhost:5042/swagger".