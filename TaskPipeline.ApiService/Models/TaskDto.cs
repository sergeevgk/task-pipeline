﻿namespace TaskPipeline.ApiService.Models;

public class TaskDto
{
	public int Id { get; set; }
	public required string Name { get; set; }
	/// <summary>
	/// Estimated average run time of the task in a pipeline in seconds.
	/// </summary>
	/// <example>10.0</example>
	public double AverageTime { get; set; }
	public string? Description { get; set; }
	public DateTime CreatedDate { get; set; }
	public bool ShouldCompleteUnsuccessfully { get; set; } = false;
}
