using Microsoft.EntityFrameworkCore;
using TaskPipeline.ApiService;
using Models = TaskPipeline.ApiService.Models;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Configure in-memory database
builder.Services.AddDbContext<TaskDbContext>(options =>
	options.UseInMemoryDatabase("TaskDb"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("/", () => "Task API");

var taskApi = app.MapGroup("/tasks");

// CRUD routes
taskApi.MapGet("/", GetAllTasks);
taskApi.MapGet("/{id:int}", GetTaskById);
taskApi.MapPost("/", CreateTask);
taskApi.MapPut("/{id:int}", UpdateTask);
taskApi.MapDelete("/{id:int}", DeleteTask);

app.Run();

// Handler implementations

static async Task<IResult> GetAllTasks(TaskDbContext db)
{
	var tasks = await db.Tasks.ToListAsync();
	return Results.Ok(tasks);
}

static async Task<IResult> GetTaskById(int id, TaskDbContext db)
{
	var task = await db.Tasks.FindAsync(id);
	return task is not null ? Results.Ok(task) : Results.NotFound();
}

static async Task<IResult> CreateTask(Models.Task task, TaskDbContext db)
{
	db.Tasks.Add(task);
	await db.SaveChangesAsync();
	return Results.Created($"/tasks/{task.Id}", task);
}

static async Task<IResult> UpdateTask(int id, Models.Task updatedTask, TaskDbContext db)
{
	var task = await db.Tasks.FindAsync(id);
	if (task is null) return Results.NotFound();

	task.Name = updatedTask.Name;
	task.AverageTime = updatedTask.AverageTime;
	task.Description = updatedTask.Description;
	task.CreatedBy = updatedTask.CreatedBy;
	task.CreatedDate = updatedTask.CreatedDate;

	await db.SaveChangesAsync();
	return Results.NoContent();
}

static async Task<IResult> DeleteTask(int id, TaskDbContext db)
{
	var task = await db.Tasks.FindAsync(id);
	if (task is null) return Results.NotFound();

	db.Tasks.Remove(task);
	await db.SaveChangesAsync();
	return Results.NoContent();
}