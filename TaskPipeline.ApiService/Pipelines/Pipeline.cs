using System.ComponentModel;

namespace TaskPipeline.ApiService.Pipelines;

public enum PipelineStatus
{
	[Description("Initial state")]
	None,
	[Description("Pipeline run is in progress")]
	Running,
	[Description("Pipeline run is finished")]
	Finished,
	[Description("Pipeline run failed")]
	Failed,
	[Description("Pipeline run canceled")] 
	Canceled
}

public class Pipeline
{
	public int Id { get; set; }
	public required string Name { get; set; }
	/// <summary>
	/// Total time of the pipeline run based on individual times of included tasks, measured in seconds.
	/// Recalculated automatically on adding a new task to a pipeline and is saved in DB.
	/// </summary>
	public double AverageRunTime => Items.Sum(i => i.Task.AverageTime);
	
	public DateTime? StartTime { get; set; }
	public DateTime? CompleteTime { get; set; }
	/// <summary>
	/// Pipeline latest run time calculated as part of Pipeline run process.
	/// Measured in seconds.
	/// </summary>
	public double LastRunTime { get; set; }
	public string? Description { get; set; }
	public PipelineStatus Status { get; set; }
	public List<PipelineItem> Items { get; set; } = [];
}
