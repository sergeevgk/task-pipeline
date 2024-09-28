using System.ComponentModel;
namespace TaskPipeline.ApiService.Models;

public record Pipeline
{
	public int Id { get; set; }
	public required string Name { get; set; }
	public List<Task> Tasks { get; set; } = [];
	/// <summary>
	/// Total time of the pipeline run based on individual times of included tasks.
	/// Recalculated automatically on adding a new task to a pipeline and is saved in DB.
	/// </summary>
	public double TotalTime => Tasks.Sum(t => t.AverageTime);
	public string? Description { get; set; }
	public PipelineStatus Status { get; set; }

}

public enum PipelineStatus
{
	[Description("Initial state")]
	None,
	[Description("Pipeline run is in progress")]
	Running,
	[Description("Pipeline run is finished")]
	Finished
}