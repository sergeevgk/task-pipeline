﻿using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TaskPipeline.ApiService.Pipelines;
using TaskPipeline.ApiService.Tasks;

namespace TaskPipeline.ApiService;
public class ExamplesSchemaFilter : ISchemaFilter
{
	public void Apply(OpenApiSchema schema, SchemaFilterContext context)
	{
		if (context.Type == typeof(ExecutableTaskDto))
		{
			schema.Example = new OpenApiObject
			{
				["id"] = new OpenApiInteger(0),
				["name"] = new OpenApiString("Sample Task"),
				["averageTime"] = new OpenApiDouble(0),
				["description"] = new OpenApiString("This is a sample task description."),
				["createdDate"] = new OpenApiDateTime(DateTime.UtcNow),
				["shouldCompleteUnsuccessfully"] = new OpenApiBoolean(false)
			};
		}

		if (context.Type == typeof(PipelineDto))
		{
			schema.Example = new OpenApiObject
			{
				["id"] = new OpenApiInteger(0),
				["name"] = new OpenApiString("Sample Pipeline"),
				["description"] = new OpenApiString("This is a sample pipeline description."),
				["status"] = new OpenApiString("None")
			};
		}
	}
}

