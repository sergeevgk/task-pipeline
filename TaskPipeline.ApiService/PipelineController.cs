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

		#region pipeline CRUD
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
		#endregion

		#region pipeline tasks operations
		// POST: /pipelines/{pipelineId}/tasks/{taskId}
		[HttpPost("{pipelineId:int}/tasks/{taskId:int}")]
		public async Task<IActionResult> AddTaskToPipeline([FromRoute] int pipelineId, [FromRoute] int taskId)
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

		// DELETE: /pipelines/{pipelineId}/tasks/{taskId}
		[HttpDelete("{pipelineId:int}/tasks/{taskId:int}")]
		public async Task<IActionResult> RemoveTaskFromPipeline([FromRoute] int pipelineId, [FromRoute] int taskId)
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

			var task = pipeline.Tasks.FirstOrDefault(t => t.Id == taskId);
			if (task is null)
				return NotFound($"Task with id [{taskId}] is not found in pipeline [{pipelineId}]");

			pipeline.Tasks.Remove(task);
			task.PipelineId = 0;
			task.Pipeline = null;

			await _appDbContext.SaveChangesAsync();
			return NoContent();
		}
		#endregion

		// GET: /pipelines/{id}/time
		[HttpGet("{id:int}/time")]
		public async Task<IActionResult> GetPipelineTotalTimeById(int id)
		{
			var pipeline = await _appDbContext.Pipelines.FirstOrDefaultAsync(p => p.Id == id);
			// TotalTime actually sums the Task.AverageTime for all tasks assigned to a pipeline. It is recalculated on adding a new task to a pipeline.
			// alternative implementation - use Pipelines.Include(p => p.Tasks) and call pipeline.Tasks.Sum(t => t.AverageTime);
			return pipeline is not null ? Ok(pipeline.TotalTime) : NotFound();
		}

		[HttpPost("{pipelineId}/run")]
		public async Task<IActionResult> RunPipeline(int pipelineId)
		{
			var pipeline = await _appDbContext.Pipelines.Include(p => p.Tasks).FirstOrDefaultAsync(p => p.Id == pipelineId);
			if (pipeline == null)
			{
				return NotFound(new { message = "Pipeline not found." });
			}

			if (pipeline.Status == PipelineStatus.Running)
			{
				return Conflict(new { message = "Pipeline is already running." });
			}

			pipeline.Status = PipelineStatus.Running;
			await _appDbContext.SaveChangesAsync();
			double actualRunTime = 0;
			try
			{
				actualRunTime = await pipeline.RunAsync();
			}
			catch (Exception)
			{
				// TODO: log some details
				throw;
			}

			pipeline.PipelineRunTime = actualRunTime;
			pipeline.Status = PipelineStatus.Finished;
			await _appDbContext.SaveChangesAsync();

			return Ok($"Pipeline has finished running in {actualRunTime} seconds.");
			// TODO: return accepted and subscribe to the result instead of waiting
			// return Accepted(new { message = "Pipeline run has been started", pipelineId = pipelineId, status = pipeline.Status });
		}
	}
}
