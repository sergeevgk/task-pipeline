namespace TaskPipeline.ApiService;

public interface ITaskRunManager
{
	public Task RunAsync(Models.PipelineItem task);
	public Task<double> RunBatchAsync(List<Models.PipelineItem> tasks);
	public Task<double> RunSequentialAsync(List<Models.PipelineItem> tasks);
}
