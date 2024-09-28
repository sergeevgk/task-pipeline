using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TaskPipeline.ApiService
{
	[ApiController]
	[AllowAnonymous]
	[Route("tasks")]
	public class TaskController : ControllerBase
	{
		private AppDbContext _context;
		private readonly UserService _userService;

		public TaskController(AppDbContext context, UserService userService)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
		}

		[HttpGet]
		public async Task<IResult> GetAllTasks()
		{
			var tasks = await _context.Tasks.ToListAsync();
			return Results.Ok(tasks);
		}

		// GET: /tasks/{id}
		[HttpGet("{id:int}")]
		public async Task<IActionResult> GetTaskById(int id)
		{
			var task = await _context.Tasks.FindAsync(id);
			return task is not null ? Ok(task) : NotFound();
		}

		// GET: /tasks/{id}
		[HttpGet("{id:int}/time")]
		public async Task<IActionResult> GetAverageTaskTimeById(int id)
		{
			var task = await _context.Tasks.FindAsync(id);
			return task is not null ? Ok(task.AverageTime) : NotFound();
		}

		// POST: /tasks
		[HttpPost]
		public async Task<IActionResult> CreateTask([FromBody] Models.Task task)
		{
			var apiKey = Request.Headers["UserApiKey"].ToString();
			var isValid = apiKey != null && _userService.VerifyToken(apiKey);
			if (!isValid)
			{
				return Unauthorized("Invalid API token.");
			}

			_context.Tasks.Add(task);
			await _context.SaveChangesAsync();
			return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
		}

		// PUT: /tasks/{id}
		[HttpPut("{id:int}")]
		public async Task<IActionResult> UpdateTask(int id, [FromBody] Models.Task updatedTask)
		{
			var apiKey = Request.Headers["UserApiKey"].ToString();
			var isValid = apiKey != null && _userService.VerifyToken(apiKey);
			if (!isValid)
			{
				return Unauthorized("Invalid API token.");
			}

			var task = await _context.Tasks.FindAsync(id);
			if (task is null)
				return NotFound();

			task.Name = updatedTask.Name;
			task.AverageTime = updatedTask.AverageTime;
			task.Description = updatedTask.Description;
			task.CreatedBy = updatedTask.CreatedBy;
			task.CreatedDate = updatedTask.CreatedDate;

			await _context.SaveChangesAsync();
			return NoContent();
		}

		// DELETE: /tasks/{id}
		[HttpDelete("{id:int}")]
		public async Task<IActionResult> DeleteTask(int id)
		{
			var apiKey = Request.Headers["UserApiKey"].ToString();
			var isValid = apiKey != null && _userService.VerifyToken(apiKey);
			if (!isValid)
			{
				return Unauthorized("Invalid API token.");
			}

			var task = await _context.Tasks.FindAsync(id);
			if (task is null)
				return NotFound();

			_context.Tasks.Remove(task);
			await _context.SaveChangesAsync();
			return NoContent();
		}
	}
}
