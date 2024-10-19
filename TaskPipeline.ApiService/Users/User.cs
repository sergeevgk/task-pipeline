namespace TaskPipeline.ApiService.Users;

public record User
{
	public int Id { get; set; }
	public required string Name { get; set; }
	/// <summary>
	/// For the beginning, token is just stored in a db column.
	/// </summary>
	public required string ApiToken { get; set; }
}
