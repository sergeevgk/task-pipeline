
using System.Collections.Concurrent;

namespace TaskPipeline.ApiService.Pipelines
{
	public class PipelineManager : IPipelineManager
	{
		private readonly ConcurrentDictionary<int, CancellationTokenSource> _cancellationTokenSourceDictionary;

		public PipelineManager()
		{
			_cancellationTokenSourceDictionary = new ConcurrentDictionary<int, CancellationTokenSource>();
		}

		public CancellationToken IssuePipelineRunCancellationToken(int pipelineId)
		{
			var hasCancellationTokenSource = _cancellationTokenSourceDictionary.TryGetValue(pipelineId, out var cancellationTokenSource);
			if (!hasCancellationTokenSource)
			{
				cancellationTokenSource = new CancellationTokenSource();
				_cancellationTokenSourceDictionary.TryAdd(pipelineId, cancellationTokenSource);
			}

			return cancellationTokenSource.Token;
		}

		public bool TryStopPipeline(int pipelineId)
		{
			if (_cancellationTokenSourceDictionary.TryRemove(pipelineId, out var cancellationTokenSource))
			{
				cancellationTokenSource.Cancel();
				return true;
			}

			return false;
		}

		public bool TryUtilizePipelineRunCancellationToken(int pipelineId)
		{
			return _cancellationTokenSourceDictionary.TryRemove(pipelineId, out var cancellationTokenSource);
		}
	}
}
