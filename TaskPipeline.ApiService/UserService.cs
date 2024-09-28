namespace TaskPipeline.ApiService;

public class UserService
{
	private UserDbContext _context;
	public UserService(UserDbContext context)
	{
		_context = context;
	}

	public bool VerifyToken(string userToken)
	{
		var user = _context.Users.FirstOrDefault(user => user.ApiToken == userToken);
		return user != null;
	}
}
