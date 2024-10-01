using TaskPipeline.ApiService.Tasks;

namespace TaskPipeline.ApiService.Pipelines;

public record PipelineItem
{
	public int Id { get; set; }
	public required int TaskId { get; init; }
	public ExecutableTask Task { get; init; }
	public required int PipelineId { get; init; }
	public Pipeline Pipeline { get; init; }
}
