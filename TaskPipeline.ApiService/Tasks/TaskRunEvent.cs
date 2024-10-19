using TaskPipeline.ApiService.Pipelines;

namespace TaskPipeline.ApiService.Tasks;

public record TaskRunEvent
{
	// TODO: refactor this, I better wouldn't send a full entity as part of the event.
	// Need to introduce PipelineItems collection in DB context
	public PipelineItem PipelineItem { get; set; }
	public string Source { get; set; }
	public DateTime? StartTime { get; set; }
}
