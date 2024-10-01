using TaskPipeline.ApiService.Pipelines;

namespace TaskPipeline.ApiService.Tasks;

public interface ITaskRunManager
{
	public Task RunAsync(PipelineItem task);
	public Task<double> RunBatchAsync(List<PipelineItem> tasks);
	public Task<double> RunSequentialAsync(List<PipelineItem> tasks);
}
