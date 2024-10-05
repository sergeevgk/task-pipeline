using TaskPipeline.ApiService.Pipelines;

namespace TaskPipeline.ApiService.Tasks;

public interface ITaskRunManager
{
	/// <summary>
	/// Runs the task and calculates its actual run time in seconds in case of successful completion.
	/// Calculates time of individual task runs.
	/// </summary>
	/// <param name="task"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task tun time in seconds</returns>
	public Task<double> RunAsync(PipelineItem task, CancellationToken cancellationToken);

	/// <summary>
	/// Runs tasks in a batch - sequence is not guaranteed. Returns summary task run time in seconds.
	/// </summary>
	/// <param name="tasks"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<double> RunBatchAsync(List<PipelineItem> tasks, CancellationToken cancellationToken);

	/// <summary>
	/// Runs tasks sequentially. Returns summary task run time in seconds.
	/// </summary>
	/// <param name="tasks"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public Task<double> RunSequentialAsync(List<PipelineItem> tasks, CancellationToken cancellationToken);
}
