using Microsoft.EntityFrameworkCore;

namespace TaskPipeline.ApiService
{
    public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options)
		: base(options) { }

		public DbSet<Models.Task> Tasks => Set<Models.Task>();
		public DbSet<Models.Pipeline> Pipelines => Set<Models.Pipeline>();
	}
}
