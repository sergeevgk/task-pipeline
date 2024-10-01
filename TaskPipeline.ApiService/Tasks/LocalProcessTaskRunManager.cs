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

	public async Task RunAsync(PipelineItem item)
	{
		var task = item.Task;
		if (task == null)
		{
			throw new ArgumentNullException($"{nameof(item.Task)} is null");
		}

		if (string.IsNullOrEmpty(_settings.ExternalConsoleAppPath))
		{
			Console.WriteLine("Configuration for external program is missing. Running a dummy task instead.");

			var taskRunTimeInMs = (int)Math.Ceiling(task.AverageTime * 1000);
			// emulate some task running asynchronously
			await Task.Delay(taskRunTimeInMs);

			Console.WriteLine("Dummy tusk run is completed.");
			return;
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

		using (var process = new Process { StartInfo = processInfo })
		{
			process.Start();
			string output = await process.StandardOutput.ReadToEndAsync();
			string errors = await process.StandardError.ReadToEndAsync();
			await process.WaitForExitAsync();

			// Visual studio debugger shows exitCode=0 for a process that terminated due to unhandled exception. Check for errors to manage this in debug
			// https://github.com/dotnet/runtime/issues/35599
			if (process.ExitCode != 0 || !string.IsNullOrEmpty(errors))
			{
				throw new Exception($"Task failed with exit code {process.ExitCode}: {errors}");
			}

			Console.WriteLine(output);
		}
	}

	/// <summary>
	/// Runs several tasks. For now simply Task.WhenAll() RunTask actions for every task in the list.
	/// Maybe there is a way to optimize.
	/// </summary>
	/// <param name="tasks"></param>
	/// <returns>Pipeline run time in seconds.</returns>
	public async Task<double> RunBatchAsync(List<PipelineItem> tasks)
	{
		var watch = Stopwatch.StartNew();

		await Task.WhenAll(tasks.Select(t => RunAsync(t)));

		watch.Stop();
		var elapsedMs = watch.ElapsedMilliseconds;
		var pipelineRunTimeInSeconds = Math.Ceiling(1.0 * elapsedMs / 1000);

		return pipelineRunTimeInSeconds;
	}

	/// <summary>
	/// Runs several tasks. For now simply calls RunTask for every task in the list sequentially.
	/// </summary>
	/// <param name="tasks"></param>
	/// <returns>Pipeline run time in seconds.</returns>
	public async Task<double> RunSequentialAsync(List<PipelineItem> tasks)
	{
		var watch = Stopwatch.StartNew();

		foreach (var task in tasks)
		{
			await RunAsync(task);
		}

		watch.Stop();
		var elapsedMs = watch.ElapsedMilliseconds;
		var pipelineRunTimeInSeconds = Math.Ceiling(1.0 * elapsedMs / 1000);

		return pipelineRunTimeInSeconds;
	}
}
