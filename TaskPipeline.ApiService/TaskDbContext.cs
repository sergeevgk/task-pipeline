using Microsoft.EntityFrameworkCore;
using TaskPipeline.ApiService.Models;

namespace TaskPipeline.ApiService
{
    public class TaskDbContext : DbContext
	{
		public TaskDbContext(DbContextOptions<TaskDbContext> options)
		: base(options) { }

		public DbSet<Models.Task> Tasks => Set<Models.Task>();
	}
}
