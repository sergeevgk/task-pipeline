using System.ComponentModel;
using ThreadingTasks = System.Threading.Tasks;
namespace TaskPipeline.ApiService.Models;
public enum PipelineStatus
{
	[Description("Initial state")]
	None,
	[Description("Pipeline run is in progress")]
	Running,
	[Description("Pipeline run is finished")]
	Finished
}

public class Pipeline
{
	public int Id { get; set; }
	public required string Name { get; set; }
	public List<Task> Tasks { get; set; } = [];
	/// <summary>
	/// Total time of the pipeline run based on individual times of included tasks.
	/// Recalculated automatically on adding a new task to a pipeline and is saved in DB.
	/// </summary>
	public double TotalTime => Tasks.Sum(t => t.AverageTime);
	/// <summary>
	/// Pipeline actual run time calculated as part of <see cref="RunAsync"/> process.
	/// Measured in seconds.
	/// </summary>
	public double PipelineRunTime { get; set; }
	public string? Description { get; set; }
	public PipelineStatus Status { get; set; }

	/// <summary>
	/// Runs the Tasks included in the pipeline.
	/// </summary>
	/// <returns>Pipeline run time.</returns>
	public async Task<double> RunAsync() 
	{
		var watch = System.Diagnostics.Stopwatch.StartNew();

		// todo: revisit this as it is not sequential execution
		await ThreadingTasks.Task.WhenAll(Tasks.Select(t => t.RunAsync()));

		watch.Stop();
		var elapsedMs = watch.ElapsedMilliseconds;
		var pipelineRunTimeInSeconds = Math.Ceiling(1.0 * elapsedMs / 1000);
		return pipelineRunTimeInSeconds;
	}
}
