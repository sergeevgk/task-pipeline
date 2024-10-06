namespace TaskPipeline.ApiService.Pipelines
{
	public static class CollectionItemsExtensions
	{
		public static bool Is_In<T>(this T status, params T[] statuses)
		{
			if (statuses == null || statuses.Length == 0)
			{
				return false;
			}

			return statuses.Contains(status);
		}
	}
}
