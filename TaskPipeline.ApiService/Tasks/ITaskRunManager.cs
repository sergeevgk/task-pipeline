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
	public Task<double> RunAsync(ExecutableTask task, CancellationToken cancellationToken);

	/// <summary>
	/// Runs tasks in a batch - sequence is not guaranteed
	/// </summary>
	/// <param name="tasks"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task run times in seconds.</returns>
	public Task<IReadOnlyList<double>> RunBatchAsync(List<ExecutableTask> tasks, CancellationToken cancellationToken);

	/// <summary>
	/// Runs tasks sequentially.
	/// </summary>
	/// <param name="tasks"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task run times in seconds.</returns>
	public Task<IReadOnlyList<double>> RunSequentialAsync(List<ExecutableTask> tasks, CancellationToken cancellationToken);
}
