using Newtonsoft.Json;
using TaskPipeline.ApiService.Tasks;

namespace TaskPipeline.ApiService.Pipelines;

public enum PipelineItemStatus
{
	None,
	Running,
	Canceled,
	Failed,
	Finished
}

public record PipelineItem
{
	public int Id { get; set; }
	public required int TaskId { get; init; }
	public ExecutableTask Task { get; init; }
	public required int PipelineId { get; init; }
	[JsonIgnore]
	public Pipeline Pipeline { get; init; }
	public int? NextItemId { get; set; } = null;
	[JsonIgnore]
	public PipelineItem? NextItem { get; set; } = null;
	public double LastRunTime { get; set; }
	public PipelineItemStatus Status { get; set; }

	public bool IsLastItem()
	{
		return NextItem != null;
	}
}