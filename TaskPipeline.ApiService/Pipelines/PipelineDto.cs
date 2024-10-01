namespace TaskPipeline.ApiService.Pipelines;

public class PipelineDto
{
	public int Id { get; set; }
	public required string Name { get; set; }
	public string? Description { get; set; }
	public PipelineStatus Status { get; set; }
}
