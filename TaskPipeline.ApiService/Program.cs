using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using TaskPipeline.ApiService;
using TaskPipeline.ApiService.DAL;
using TaskPipeline.ApiService.Exceptions;
using TaskPipeline.ApiService.Pipelines;
using TaskPipeline.ApiService.Tasks;
using TaskPipeline.ApiService.Users;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Configure External Programs
builder.Services.Configure<ExternalProgramSettings>(options =>
{
	builder.Configuration.GetSection("ExternalPrograms").Bind(options);
});

builder.Services.AddDbContext<AppDbContext>();

builder.Services.AddTransient<UserService>();
builder.Services.AddTransient<ITaskRunManager, LocalProcessTaskRunManager>();
builder.Services.AddSingleton<IPipelineManager, PipelineManager>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddControllers().AddNewtonsoftJson(options => {
	options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
	options.SerializerSettings.Converters.Add(new StringEnumConverter());
});

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

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapDefaultEndpoints();
app.MapControllers().AllowAnonymous();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
	app.UseDeveloperExceptionPage();

	using var scope = app.Services.CreateScope();
	var services = scope.ServiceProvider;
	var userDb = services.GetRequiredService<AppDbContext>();
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
static async Task<IResult> GetAllUsers(AppDbContext db)
{
	var users = await db.Users.ToListAsync();
	return Results.Ok(users);
}

static async Task<IResult> CreateUser(User user, AppDbContext db)
{
	db.Users.Add(user);
	await db.SaveChangesAsync();
	return Results.Created($"/users/{user.Id}", user);
}

#endregion

app.Run();