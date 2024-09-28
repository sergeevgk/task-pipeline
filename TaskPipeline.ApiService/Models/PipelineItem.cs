namespace TaskPipeline.ApiService.Models;

public record PipelineItem
{
	public int Id { get; set; }
	public required int TaskId { get; set; }
	public required Task Task { get; set; }
	public required int PipelineId { get; set; }
	public required Pipeline Pipeline { get; set; }
}
