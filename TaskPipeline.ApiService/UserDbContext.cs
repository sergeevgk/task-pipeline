using Microsoft.EntityFrameworkCore;

namespace TaskPipeline.ApiService
{
    public class UserDbContext : DbContext
	{
		public UserDbContext(DbContextOptions<UserDbContext> options)
		: base(options) { }

		public DbSet<Models.User> Users => Set<Models.User>();
	}
}
