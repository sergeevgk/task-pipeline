using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TaskPipeline.ApiService;
public class ExamplesSchemaFilter : ISchemaFilter
{
	public void Apply(OpenApiSchema schema, SchemaFilterContext context)
	{
		// Check if the schema is for the 'Task' class
		if (context.Type == typeof(Models.Task))
		{
			// Set example values for all properties of the Task class
			schema.Example = new OpenApiObject
			{
				["id"] = new OpenApiInteger(0),
				["name"] = new OpenApiString("Sample Task"),
				["averageTime"] = new OpenApiDouble(0),
				["description"] = new OpenApiString("This is a sample task description."),
				["createdBy"] = new OpenApiString("Alice"),
				["createdDate"] = new OpenApiDateTime(DateTime.UtcNow),
				["pipelineId"] = new OpenApiInteger(0),
				["pipeline"] = new OpenApiNull()
			};
		}
	}
}

