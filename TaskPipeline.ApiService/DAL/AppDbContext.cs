using Microsoft.EntityFrameworkCore;
using TaskPipeline.ApiService.Pipelines;
using TaskPipeline.ApiService.Tasks;
using TaskPipeline.ApiService.Users;

namespace TaskPipeline.ApiService.DAL;

public class AppDbContext : DbContext
{
	public DbSet<User> Users => Set<User>();
	public DbSet<ExecutableTask> Tasks => Set<ExecutableTask>();
	public DbSet<Pipeline> Pipelines => Set<Pipeline>();
	public string DbPath { get; }

	public AppDbContext(DbContextOptions<AppDbContext> options)
	: base(options) 
	{
		var folder = Environment.SpecialFolder.LocalApplicationData;
		var path = Environment.GetFolderPath(folder);
		DbPath = Path.Join(path, "task_pipeline.db");
	}

	protected override void OnConfiguring(DbContextOptionsBuilder options)
		=> options.UseSqlite($"Data Source={DbPath}");

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<User>().HasData(
			new User { Id = 1, Name = "Alice", ApiToken = "token_123" },
			new User { Id = 2, Name = "Bob", ApiToken = "token_456" },
			new User { Id = 3, Name = "Charlie", ApiToken = "token_789" });
	}
}
