using TaskPipeline.ApiService.Pipelines;

namespace TaskPipeline.ApiService.Tasks;

public interface ITaskRunManager
{
	public Task RunAsync(PipelineItem task, CancellationToken cancellationToken);
	public Task<double> RunBatchAsync(List<PipelineItem> tasks, CancellationToken cancellationToken);
	public Task<double> RunSequentialAsync(List<PipelineItem> tasks, CancellationToken cancellationToken);
}
