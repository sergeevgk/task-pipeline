using Microsoft.Extensions.Options;
using System.Diagnostics;
using TaskPipeline.ApiService.Pipelines;

namespace TaskPipeline.ApiService.Tasks;

public class LocalProcessTaskRunManager : ITaskRunManager
{
	private readonly ExternalProgramSettings _settings;

	public LocalProcessTaskRunManager(IOptions<ExternalProgramSettings> settings)
	{
		_settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
	}

	/// <inheritdoc/>
	public async Task<double> RunAsync(ExecutableTask task, CancellationToken cancellationToken)
	{
		if (task == null)
		{
			throw new ArgumentNullException($"{nameof(Task)} is null");
		}

		if (string.IsNullOrEmpty(_settings.ExternalConsoleAppPath))
		{
			Console.WriteLine("Configuration for external program is missing. Running a dummy task instead.");

			var taskRunTimeInMs = (int)Math.Ceiling(task.AverageTime * 1000);
			// emulate some task running asynchronously
			await Task.Delay(taskRunTimeInMs, cancellationToken);

			Console.WriteLine("Dummy tusk run is completed.");
			return task.AverageTime;
		}

		var externalProgramPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, _settings.ExternalConsoleAppPath));
		var commandLineArgumentsFromTask = $"--delay {task.AverageTime}" + (task.ShouldCompleteUnsuccessfully ? " --throw" : "");
		var processInfo = new ProcessStartInfo
		{
			FileName = externalProgramPath,
			Arguments = commandLineArgumentsFromTask,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};
		
		if (cancellationToken.IsCancellationRequested)
		{
			throw new OperationCanceledException($"Pipeline task {task.Id} was canceled before starting an external process.");
		}

		using var process = new Process { StartInfo = processInfo };
		process.Start();
		using var registration = cancellationToken.Register(process.Kill);

		string output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
		string errors = await process.StandardError.ReadToEndAsync(cancellationToken);
		await process.WaitForExitAsync(cancellationToken);

		// not sure if the OperationCanceledException should be thrown explicitly here.
		if (cancellationToken.IsCancellationRequested)
		{
			throw new OperationCanceledException($"Pipeline task {task.Id} was canceled with the external process {externalProgramPath}.");
		}

		// Visual studio debugger shows exitCode=0 for a process that terminated due to unhandled exception. Check for errors to manage this in debug
		// https://github.com/dotnet/runtime/issues/35599
		if (process.ExitCode != 0 || !string.IsNullOrEmpty(errors))
		{
			throw new Exception($"Task failed with exit code {process.ExitCode}: {errors}");
		}

		Console.WriteLine(output);
		var elapsedTimeSeconds = (process.StartTime - process.ExitTime).TotalSeconds;
		Console.WriteLine($"Process run took {elapsedTimeSeconds} seconds.");

		return elapsedTimeSeconds;
	}

	/// <summary>
	/// Runs several tasks. For now simply Task.WhenAll() RunTask actions for every task in the list.
	/// Maybe there is a way to optimize.
	/// Calculates the individual run times of each task.
	/// </summary>
	/// <param name="tasks"></param>
	/// <returns>Readonly collection of individual tasks run time in seconds.</returns>
	public async Task<IReadOnlyList<double>> RunBatchAsync(List<ExecutableTask> tasks, CancellationToken cancellationToken)
	{
		var executionTimes = await Task.WhenAll(tasks.Select(t => RunAsync(t, cancellationToken)));

		return executionTimes;
	}

	/// <summary>
	/// Runs several tasks. For now simply calls RunTask for every task in the list sequentially.
	/// Calculates the individual run times of each task.
	/// </summary>
	/// <param name="tasks"></param>
	/// <returns>Readonly collection of individual tasks run time in seconds.</returns>
	public async Task<IReadOnlyList<double>> RunSequentialAsync(List<ExecutableTask> tasks, CancellationToken cancellationToken)
	{
		var executionTimes = new List<double>();
		foreach (var task in tasks)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var executionTime = await RunAsync(task, cancellationToken);
			executionTimes.Add(executionTime);
		}

		return executionTimes;
	}
}
