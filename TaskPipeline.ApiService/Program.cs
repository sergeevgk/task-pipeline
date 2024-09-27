using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TaskPipeline.ApiService;
using TaskPipeline.ApiService.Models;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddDbContext<TaskDbContext>(options =>
	options.UseInMemoryDatabase("TaskDb"));
builder.Services.AddDbContext<UserDbContext>(options =>
	options.UseInMemoryDatabase("UserDb"));

builder.Services.AddTransient<UserService>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
	option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
	option.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
	{
		Type = SecuritySchemeType.ApiKey,
		In = ParameterLocation.Header,
		Name = "UserApiKey",
		Description = "Please enter a user API key"
	});
	option.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type=ReferenceType.SecurityScheme,
					Id="ApiKey"
				}
			},
			new string[]{}
		}
	});
});

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapControllers().AllowAnonymous();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("/", () => "Check the Swagger documentation in the /swagger endpoint");

#region user endpoints
// User API Group
var userApi = app.MapGroup("/users");
userApi.MapGet("/", GetAllUsers);
userApi.MapPost("/", CreateUser);

// User Handlers
static async Task<IResult> GetAllUsers(UserDbContext db)
{
	var users = await db.Users.ToListAsync();
	return Results.Ok(users);
}

static async Task<IResult> CreateUser(User user, UserDbContext db)
{
	db.Users.Add(user);
	await db.SaveChangesAsync();
	return Results.Created($"/users/{user.Id}", user);
}

using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	var userDb = services.GetRequiredService<UserDbContext>();

	if (!userDb.Users.Any())
	{
		userDb.Users.AddRange(
			new User { Name = "Alice", ApiToken = "token_123" },
			new User { Name = "Bob", ApiToken = "token_456" },
			new User { Name = "Charlie", ApiToken = "token_789" }
		);
		await userDb.SaveChangesAsync();
	}
}

#endregion

app.Run();