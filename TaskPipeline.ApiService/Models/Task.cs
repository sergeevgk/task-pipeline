namespace TaskPipeline.ApiService.Models;

public record Task
{
	public int Id { get; set; }
	public required string Name { get; set; }
	/// <summary>
	/// Estimated average run time of the task in a pipeline.
	/// </summary>
	public double AverageTime { get; set; }
	public string? Description { get; set; }
	/// <summary>
	/// User that created the task.
	/// </summary>
	public required string CreatedBy { get; set; }
	public DateTime CreatedDate { get; set; }
	public int PipelineId { get; set; }
	public Pipeline? Pipeline { get; set; }
}
