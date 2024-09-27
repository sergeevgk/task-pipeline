using Microsoft.EntityFrameworkCore;
using TaskPipeline.ApiService.Models;

namespace TaskPipeline.ApiService
{
    public class UserDbContext : DbContext
	{
		public UserDbContext(DbContextOptions<UserDbContext> options)
		: base(options) { }

		public DbSet<User> Users => Set<User>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<User>().HasData(
				new User { Id = 1, Name = "Alice", ApiToken = "token_123" },
				new User { Id = 2, Name = "Bob", ApiToken = "token_456" },
				new User { Id = 3, Name = "Charlie", ApiToken = "token_789" });
		}
	}
}
