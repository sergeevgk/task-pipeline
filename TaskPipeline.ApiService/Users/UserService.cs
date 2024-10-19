using TaskPipeline.ApiService.DAL;

namespace TaskPipeline.ApiService.Users;

public class UserService
{
	private AppDbContext _context;
	public UserService(AppDbContext context)
	{
		_context = context;
	}

	public bool VerifyToken(string userToken)
	{
		var user = GetUserByToken(userToken);
		return user != null;
	}

	public User GetUserByToken(string userToken)
	{
		var user = _context.Users.FirstOrDefault(user => user.ApiToken == userToken);
		return user;
	}
}
