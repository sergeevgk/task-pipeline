namespace TaskPipeline.ApiService.Pipelines
{
	public interface IPipelineManager
	{
		public CancellationToken IssuePipelineRunCancellationToken(int pipelineId);

		public bool TryStopPipeline(int pipelineId);

		public bool TryUtilizePipelineRunCancellationToken(int pipelineId);
	}
}
