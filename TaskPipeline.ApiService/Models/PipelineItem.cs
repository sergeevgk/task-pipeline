namespace TaskPipeline.ApiService.Models;

public record PipelineItem
{
	public int Id { get; set; }
	public required int TaskId { get; init; }
	public Task Task { get; init; }
	public required int PipelineId { get; init; }
	public Pipeline Pipeline { get; init; }
}
