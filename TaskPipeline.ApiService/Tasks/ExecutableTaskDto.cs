using System.ComponentModel.DataAnnotations;

namespace TaskPipeline.ApiService.Tasks;

public class ExecutableTaskDto
{
	public int Id { get; set; }
	public required string Name { get; set; }
	/// <summary>
	/// Estimated average run time of the task in a pipeline in seconds.
	/// </summary>
	/// <example>10.0</example>
	[Range(0, 86400)]
	public double AverageTime { get; set; } = 0;
	public string? Description { get; set; }
	public DateTime CreatedDate { get; set; }
	public bool ShouldCompleteUnsuccessfully { get; set; } = false;
}
