namespace TaskPipeline.ApiService.Pipelines;

public record PipelineRunEvent
{
	public int PipelineId { get; set; }
	public string Source { get; set; }
	public DateTime? StartTime { get; set; }
}
