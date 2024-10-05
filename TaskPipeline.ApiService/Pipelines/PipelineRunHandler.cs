using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Threading.Channels;
using TaskPipeline.ApiService.DAL;
using TaskPipeline.ApiService.Tasks;

namespace TaskPipeline.ApiService.Pipelines;

public class PipelineRunHandler : BackgroundService
{
	private readonly Channel<PipelineRunEvent> _pipelineRunChannel;
	private readonly Channel<TaskRunEvent> _taskRunChannel;
	private readonly IPipelineManager _pipelineManager;
	private readonly IServiceScopeFactory _serviceProviderFactory;

	public PipelineRunHandler(Channel<PipelineRunEvent> channel,
		IPipelineManager pipelineManager,
		Channel<TaskRunEvent> taskRunChannel,
		IServiceScopeFactory serviceProviderFactory)
	{
		_pipelineRunChannel = channel ?? throw new ArgumentNullException(nameof(channel));
		_pipelineManager = pipelineManager ?? throw new ArgumentNullException(nameof(pipelineManager));
		_taskRunChannel = taskRunChannel ?? throw new ArgumentNullException(nameof(taskRunChannel));
		_serviceProviderFactory = serviceProviderFactory ?? throw new ArgumentNullException(nameof(serviceProviderFactory));
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await using var scope = _serviceProviderFactory.CreateAsyncScope();
		var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		while (!_pipelineRunChannel.Reader.Completion.IsCompleted && await _pipelineRunChannel.Reader.WaitToReadAsync(stoppingToken))
		{
			if (_pipelineRunChannel.Reader.TryRead(out var msg))
			{
				var msgSerialized = JsonConvert.SerializeObject(msg);
				Console.WriteLine($"Start processing {msgSerialized}");
				var pipeline = await appDbContext.Pipelines.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == msg.PipelineId, stoppingToken);
				if (pipeline == null)
				{
					// in a real system there would be some logic that logs an error and makes the message/event go to error queue
					Console.WriteLine($"Error processing message {msgSerialized} : pipeline {msg.PipelineId} is not found.");
					continue;
				}

				var pipelineCancellationToken = _pipelineManager.IssuePipelineRunCancellationToken(pipeline.Id);
				if (pipelineCancellationToken.IsCancellationRequested)
				{
					Console.WriteLine($"Pipeline {pipeline.Id} is canceled before running any tasks. Don't send TaskRunEvents");
					pipeline.Status = PipelineStatus.Canceled;
					pipeline.StartTime = null;
					await appDbContext.SaveChangesAsync(stoppingToken);
					continue;
				}

				pipeline.Status = PipelineStatus.Running;
				pipeline.StartTime = DateTime.UtcNow;
				await appDbContext.SaveChangesAsync(stoppingToken);

				foreach (var item in pipeline.Items)
				{
					var taskRunEvent = new TaskRunEvent
					{
						PipelineItem = item,
						Source = msg.Source,
						StartTime = pipeline.StartTime
					};

					await _taskRunChannel.Writer.WriteAsync(taskRunEvent, stoppingToken);
					Console.WriteLine("Send TaskRunEvent.");
				}
			}
		}
	}
}
