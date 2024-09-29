using Microsoft.EntityFrameworkCore;
using TaskPipeline.ApiService.Models;

namespace TaskPipeline.ApiService.DAL;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options)
	: base(options) { }

	public DbSet<User> Users => Set<User>();
	public DbSet<Models.Task> Tasks => Set<Models.Task>();
	public DbSet<Pipeline> Pipelines => Set<Pipeline>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<User>().HasData(
			new User { Id = 1, Name = "Alice", ApiToken = "token_123" },
			new User { Id = 2, Name = "Bob", ApiToken = "token_456" },
			new User { Id = 3, Name = "Charlie", ApiToken = "token_789" });
	}
}
