namespace TaskPipeline.ApiService.Pipelines;

public enum PipelineCompleteStatus
{
	None,
	Success,
	Failed,
	Canceled
}

public record PipelineCompleteEvent
{
	public int PipelineId { get; set; }
	public string Source { get; set; }
	public PipelineCompleteStatus Status { get; set; }
	public DateTime? CompleteTime { get; set; }
}