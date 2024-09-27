using Microsoft.EntityFrameworkCore;

namespace TaskPipeline.ApiService
{
    public class PipelineDbContext : DbContext
	{
		public PipelineDbContext(DbContextOptions<PipelineDbContext> options)
		: base(options) { }

		public DbSet<Models.Pipeline> Pipelines => Set<Models.Pipeline>();
	}
}
