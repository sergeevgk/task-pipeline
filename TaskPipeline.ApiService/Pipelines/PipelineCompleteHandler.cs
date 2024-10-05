using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Threading.Channels;
using TaskPipeline.ApiService.DAL;

namespace TaskPipeline.ApiService.Pipelines;

public class PipelineCompleteHandler : BackgroundService
{
	private readonly Channel<PipelineCompleteEvent> _pipelineCompleteChannel;
	private readonly IPipelineManager _pipelineManager;
	private readonly IServiceScopeFactory _serviceProviderFactory;

	public PipelineCompleteHandler(Channel<PipelineCompleteEvent> channel,
		IPipelineManager pipelineManager,
		IServiceScopeFactory serviceScopeFactory)
	{
		_pipelineCompleteChannel = channel ?? throw new ArgumentNullException(nameof(channel));
		_pipelineManager = pipelineManager ?? throw new ArgumentNullException(nameof(pipelineManager));
		_serviceProviderFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await using var scope = _serviceProviderFactory.CreateAsyncScope();
		var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		while (!_pipelineCompleteChannel.Reader.Completion.IsCompleted && await _pipelineCompleteChannel.Reader.WaitToReadAsync(stoppingToken))
		{
			if (_pipelineCompleteChannel.Reader.TryRead(out var msg))
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

				pipeline.CompleteTime = msg.CompleteTime;
				pipeline.Status = msg.Status switch
				{
					PipelineCompleteStatus.Success => PipelineStatus.Finished,
					PipelineCompleteStatus.Failed => PipelineStatus.Failed,
					PipelineCompleteStatus.Canceled => PipelineStatus.Canceled,
					_ => throw new ApplicationException($"Unexpected pipeline status {msg.Status}."),
				};

				// pipeline run time including all message waiting and processing time which is fair
				// another way is to sum up individual items' times
				pipeline.LastRunTime = (pipeline.CompleteTime - pipeline.StartTime)?.TotalSeconds ?? 0;

				if (!_pipelineManager.TryUtilizePipelineRunCancellationToken(msg.PipelineId))
				{
					Console.WriteLine($"Error utilizing the pipeline {msg.PipelineId} cancellation token.");
				}

				Console.WriteLine($"Pipeline {msg.PipelineId} has finished running in {0} seconds.");
				// decide how to notify the user about pipeline run finish
				// TODO: webhook / singnalR / websocket / message or event in special queue

				// just save to DB and allow to read results.
				await appDbContext.SaveChangesAsync(stoppingToken);
			}
		}
	}
}
