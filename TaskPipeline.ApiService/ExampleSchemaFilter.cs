using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TaskPipeline.ApiService;
public class ExamplesSchemaFilter : ISchemaFilter
{
	public void Apply(OpenApiSchema schema, SchemaFilterContext context)
	{
		if (context.Type == typeof(Models.Task))
		{
			schema.Example = new OpenApiObject
			{
				["id"] = new OpenApiInteger(0),
				["name"] = new OpenApiString("Sample Task"),
				["averageTime"] = new OpenApiDouble(0),
				["description"] = new OpenApiString("This is a sample task description."),
				["createdBy"] = new OpenApiString("Alice"),
				["createdDate"] = new OpenApiDateTime(DateTime.UtcNow),
				["shouldCompleteUnsuccessfully"] = new OpenApiBoolean(false)
			};
		}

		if (context.Type == typeof(Models.Pipeline))
		{
			schema.Properties.Remove("totalTime");
			schema.Properties.Remove("pipelineRunTime");
			schema.Example = new OpenApiObject
			{
				["id"] = new OpenApiInteger(0),
				["name"] = new OpenApiString("Sample Pipeline"),
				["items"] = new OpenApiArray(),
				["description"] = new OpenApiString("This is a sample pipeline description."),
				["status"] = new OpenApiString("None")
			};
		}
	}
}

