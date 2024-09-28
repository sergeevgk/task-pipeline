using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TaskPipeline.ApiService;
using TaskPipeline.ApiService.Models;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseInMemoryDatabase("TaskPipelineDb"));
builder.Services.AddDbContext<UserDbContext>(options =>
	options.UseInMemoryDatabase("TaskPipelineDb"));

builder.Services.AddTransient<UserService>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddControllers().AddNewtonsoftJson(options =>
	options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);
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
	option.SchemaFilter<ExamplesSchemaFilter>();
});

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapControllers().AllowAnonymous();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();

	using var scope = app.Services.CreateScope();
	var services = scope.ServiceProvider;
	var userDb = services.GetRequiredService<UserDbContext>();
	userDb.Database.EnsureCreated();
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

#endregion

app.Run();