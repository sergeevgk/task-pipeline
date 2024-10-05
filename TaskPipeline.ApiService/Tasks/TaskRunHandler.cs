using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading.Channels;
using TaskPipeline.ApiService.DAL;
using TaskPipeline.ApiService.Pipelines;

namespace TaskPipeline.ApiService.Tasks;

public class TaskRunHandler : BackgroundService
{
	private readonly Channel<PipelineCompleteEvent> _pipelineCompleteChannel;
	// here we assume that the channel contains only messages for a __one particular pipeline__.
	// in a real world application it can be implemented via Kafka paritions or some other mechanisms.
	// built-in Channel<T> is just a simple abstraction to demostrate the idea. Probably it can be set up to have just one producer per pipelineID.
	private readonly Channel<TaskRunEvent> _taskRunChannel;
	private readonly IPipelineManager _pipelineManager;
	private readonly ITaskRunManager _taskRunManager;
	private readonly IServiceScopeFactory _serviceProviderFactory;

	public TaskRunHandler(Channel<PipelineCompleteEvent> channel,
		IPipelineManager pipelineManager,
		Channel<TaskRunEvent> taskRunChannel,
		ITaskRunManager taskRunManager,
		IServiceScopeFactory serviceProviderFactory)
	{
		_pipelineCompleteChannel = channel ?? throw new ArgumentNullException(nameof(channel));
		_pipelineManager = pipelineManager ?? throw new ArgumentNullException(nameof(pipelineManager));
		_taskRunChannel = taskRunChannel ?? throw new ArgumentNullException(nameof(taskRunChannel));
		_taskRunManager = taskRunManager ?? throw new ArgumentNullException(nameof(taskRunManager));
		_serviceProviderFactory = serviceProviderFactory ?? throw new ArgumentNullException(nameof(serviceProviderFactory));
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await using var scope = _serviceProviderFactory.CreateAsyncScope();
		var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		while (!_taskRunChannel.Reader.Completion.IsCompleted && await _taskRunChannel.Reader.WaitToReadAsync(stoppingToken))
		{
			if (_taskRunChannel.Reader.TryRead(out var msg))
			{
				var msgSerialized = JsonConvert.SerializeObject(msg);
				// TODO: reload from AppDbContext because of possible changes? Restrict changes on already runnning pipeline?
				var pipelineItem = msg.PipelineItem;
				Console.WriteLine($"Start processing {msgSerialized}");
				var task = await appDbContext.Tasks.FirstOrDefaultAsync(p => p.Id == pipelineItem.TaskId, stoppingToken);
				if (task == null)
				{
					// in a real system there would be some logic that logs an error and makes the message/event fall to error queue
					Console.WriteLine($"Error processing message {msgSerialized} : task {pipelineItem.TaskId} is not found.");
					continue;
				}

				var pipelineCancellationToken = _pipelineManager.IssuePipelineRunCancellationToken(pipelineItem.PipelineId);

				// for the exception cases
				var incompleteTaskRunWatch = Stopwatch.StartNew();

				double actualRunTime = 0;
				try
				{
					actualRunTime = await _taskRunManager.RunAsync(pipelineItem, pipelineCancellationToken);
				}
				catch (OperationCanceledException ex)
				{
					Console.WriteLine($"Pipeline {pipelineItem.PipelineId} was stopped: {ex.Message}");
					var canceledEvent = new PipelineCompleteEvent
					{
						PipelineId = pipelineItem.PipelineId,
						Status = PipelineCompleteStatus.Canceled,
						CompleteTime = DateTime.UtcNow,
						Source = $"{nameof(TaskRunHandler)} pipelineItem {pipelineItem.Id}"
					};

					await _pipelineCompleteChannel.Writer.WriteAsync(canceledEvent, stoppingToken);
					continue;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Pipeline run failed with error: {ex.Message}");
					var failedEvent = new PipelineCompleteEvent
					{
						PipelineId = pipelineItem.PipelineId,
						Status = PipelineCompleteStatus.Failed,
						CompleteTime = DateTime.UtcNow,
						Source = $"{nameof(TaskRunHandler)} pipelineItem {pipelineItem.Id}"
					};

					await _pipelineCompleteChannel.Writer.WriteAsync(failedEvent, stoppingToken);
					continue;
				}
				finally
				{
					incompleteTaskRunWatch.Stop();
					var elapsedSeconds = incompleteTaskRunWatch.Elapsed.TotalSeconds;
					pipelineItem.LastRunTime = actualRunTime == 0 ? elapsedSeconds : actualRunTime;
					await appDbContext.SaveChangesAsync(stoppingToken);
				}

				if (pipelineItem.IsLastItem())
				{
					Console.WriteLine($"Pipeline {pipelineItem.PipelineId} run is completed.");
					var completeEvent = new PipelineCompleteEvent
					{
						PipelineId = pipelineItem.PipelineId,
						Status = PipelineCompleteStatus.Success,
						CompleteTime = DateTime.UtcNow,
						Source = $"{nameof(TaskRunHandler)} pipelineItem {pipelineItem.Id}"
					};

					await _pipelineCompleteChannel.Writer.WriteAsync(completeEvent, stoppingToken);
					Console.WriteLine("Send PipelineCompleteEvent.");
				}
			}
		}
	}
}
