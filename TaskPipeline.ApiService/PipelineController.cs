using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskPipeline.ApiService.Models;

namespace TaskPipeline.ApiService
{
	[ApiController]
	[Route("pipelines")]
	public class PipelineController : ControllerBase
	{
		private readonly AppDbContext _appDbContext;
		private readonly UserService _userService;

		public PipelineController(AppDbContext taskDbContext, UserService userService)
		{
			_appDbContext = taskDbContext ?? throw new ArgumentNullException(nameof(taskDbContext));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
		}

		// GET: /pipelines
		[HttpGet]
		public async Task<IActionResult> GetAllPipelines()
		{
			var pipelines = await _appDbContext.Pipelines.Include(p => p.Tasks).ToListAsync();
			return Ok(pipelines);
		}

		// GET: /pipelines/{id}
		[HttpGet("{id:int}")]
		public async Task<IActionResult> GetPipelineById(int id)
		{
			var pipeline = await _appDbContext.Pipelines.Include(p => p.Tasks).FirstOrDefaultAsync(p => p.Id == id);
			return pipeline is not null ? Ok(pipeline) : NotFound();
		}

		// GET: /pipelines/{id}/time
		[HttpGet("{id:int}/time")]
		public async Task<IActionResult> GetPipelineTotalTimeById(int id)
		{
			var pipeline = await _appDbContext.Pipelines.Include(p => p.Tasks).FirstOrDefaultAsync(p => p.Id == id);
			return pipeline is not null ? Ok(pipeline.TotalTime) : NotFound();
		}

		// POST: /pipelines
		[HttpPost]
		public async Task<IActionResult> CreatePipeline([FromBody] Pipeline pipeline)
		{
			var apiKey = Request.Headers["UserApiKey"].ToString();
			var isValid = apiKey != null && _userService.VerifyToken(apiKey);
			if (!isValid)
			{
				return Unauthorized("Invalid API token.");
			}
			_appDbContext.Pipelines.Add(pipeline);
			await _appDbContext.SaveChangesAsync();
			return CreatedAtAction(nameof(GetPipelineById), new { id = pipeline.Id }, pipeline);
		}

		// PUT: /pipelines/{id}
		[HttpPut("{id:int}")]
		public async Task<IActionResult> UpdatePipeline(int id, [FromBody] Pipeline updatedPipeline)
		{
			var apiKey = Request.Headers["UserApiKey"].ToString();
			var isValid = apiKey != null && _userService.VerifyToken(apiKey);
			if (!isValid)
			{
				return Unauthorized("Invalid API token.");
			}

			var pipeline = await _appDbContext.Pipelines.Include(p => p.Tasks).FirstOrDefaultAsync(p => p.Id == id);
			if (pipeline is null)
				return NotFound();

			pipeline.Name = updatedPipeline.Name;
			pipeline.Tasks = updatedPipeline.Tasks;
			pipeline.Description = updatedPipeline.Description;
			pipeline.Status = updatedPipeline.Status;

			await _appDbContext.SaveChangesAsync();
			return Ok(pipeline);
		}

		// POST: /pipelines/{pipelineId}/tasks/{taskId}
		[HttpPost("{pipelineId:int}/tasks/{taskId:int}")]
		public async Task<IActionResult> AddTaskToPipeline([FromRoute]int pipelineId, [FromRoute]int taskId)
		{
			var apiKey = Request.Headers["UserApiKey"].ToString();
			var isValid = apiKey != null && _userService.VerifyToken(apiKey);
			if (!isValid)
			{
				return Unauthorized("Invalid API token.");
			}

			var pipeline = await _appDbContext.Pipelines.Include(p => p.Tasks).FirstOrDefaultAsync(p => p.Id == pipelineId);
			if (pipeline is null)
				return NotFound($"Pipeline with id [{pipelineId}] is not found");

			var task = await _appDbContext.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
			if (task is null)
				return NotFound($"Task with id [{taskId}] is not found");

			pipeline.Tasks.Add(task);
			task.PipelineId = pipelineId;

			await _appDbContext.SaveChangesAsync();
			return Ok(pipeline);
		}

		[HttpPost("{pipelineId:int}/tasks")]
		public async Task<IActionResult> AddTaskToPipeline([FromRoute] int pipelineId, [FromBody] Models.Task task)
		{
			var apiKey = Request.Headers["UserApiKey"].ToString();
			var isValid = apiKey != null && _userService.VerifyToken(apiKey);
			if (!isValid)
			{
				return Unauthorized("Invalid API token.");
			}

			var pipeline = await _appDbContext.Pipelines.Include(p => p.Tasks).FirstOrDefaultAsync(p => p.Id == pipelineId);
			if (pipeline is null)
				return NotFound($"Pipeline with id [{pipelineId}] is not found");

			if (task is null)
				return BadRequest("Please provide a valid task");

			pipeline.Tasks.Add(task);
			task.PipelineId = pipelineId;

			await _appDbContext.SaveChangesAsync();
			return Ok(pipeline);
		}

		// DELETE: /pipelines/{id}
		[HttpDelete("{id:int}")]
		public async Task<IActionResult> DeletePipeline(int id)
		{
			var apiKey = Request.Headers["UserApiKey"].ToString();
			var isValid = apiKey != null && _userService.VerifyToken(apiKey);
			if (!isValid)
			{
				return Unauthorized("Invalid API token.");
			}

			var pipeline = await _appDbContext.Pipelines.FindAsync(id);
			if (pipeline is null)
				return NotFound();

			_appDbContext.Pipelines.Remove(pipeline);
			await _appDbContext.SaveChangesAsync();
			return NoContent();
		}
	}
}
