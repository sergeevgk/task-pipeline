using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskPipeline.ApiService.DAL;
using TaskPipeline.ApiService.Tasks;
using TaskPipeline.ApiService.Users;

namespace TaskPipeline.ApiService.Pipelines;

[ApiController]
[Route("pipelines")]
public class PipelineController : ControllerBase
{
	private readonly AppDbContext _appDbContext;
	private readonly UserService _userService;
	private readonly ITaskRunManager _taskRunManager;
	private readonly IPipelineManager _pipelineManager;

	public PipelineController(AppDbContext taskDbContext, UserService userService, ITaskRunManager taskRunManager, IPipelineManager pipelineManager)
	{
		_appDbContext = taskDbContext ?? throw new ArgumentNullException(nameof(taskDbContext));
		_userService = userService ?? throw new ArgumentNullException(nameof(userService));
		_taskRunManager = taskRunManager ?? throw new ArgumentNullException(nameof(taskRunManager));
		_pipelineManager = pipelineManager ?? throw new ArgumentNullException(nameof(pipelineManager));
	}

	#region pipeline CRUD
	// GET: /pipelines
	[HttpGet]
	public async Task<IActionResult> GetAllPipelines()
	{
		var pipelines = await _appDbContext.Pipelines
			.Include(p => p.Items)
			.ThenInclude(i => i.Task)
			.ToListAsync();

		return Ok(pipelines);
	}

	// GET: /pipelines/{id}
	[HttpGet("{id:int}")]
	public async Task<IActionResult> GetPipelineById(int id)
	{
		var pipeline = await _appDbContext.Pipelines
			.Include(p => p.Items)
			.ThenInclude(i => i.Task)
			.FirstOrDefaultAsync(p => p.Id == id);
		return pipeline is not null ? Ok(pipeline) : NotFound();
	}

	// POST: /pipelines
	[HttpPost]
	public async Task<IActionResult> CreatePipeline([FromBody] PipelineDto pipelineDto)
	{
		var apiKey = Request.Headers["UserApiKey"].ToString();
		var isValid = apiKey != null && _userService.VerifyToken(apiKey);
		if (!isValid)
		{
			return Unauthorized("Invalid API token.");
		}
		var pipeline = new Pipeline
		{
			Name = pipelineDto.Name,
			Description = pipelineDto.Description,
			Status = pipelineDto.Status
		};

		_appDbContext.Pipelines.Add(pipeline);
		await _appDbContext.SaveChangesAsync();
		return CreatedAtAction(nameof(GetPipelineById), new { id = pipeline.Id }, pipeline);
	}

	// PUT: /pipelines/{id}
	[HttpPut("{id:int}")]
	public async Task<IActionResult> UpdatePipeline(int id, [FromBody] PipelineDto updatedPipeline)
	{
		var apiKey = Request.Headers["UserApiKey"].ToString();
		var isValid = apiKey != null && _userService.VerifyToken(apiKey);
		if (!isValid)
		{
			return Unauthorized("Invalid API token.");
		}

		var pipeline = await _appDbContext.Pipelines
			.Include(p => p.Items)
			.ThenInclude(i => i.Task)
			.FirstOrDefaultAsync(p => p.Id == id);

		if (pipeline is null)
			return NotFound();

		pipeline.Name = updatedPipeline.Name;
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

	#region pipeline items operations
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

		var pipeline = await _appDbContext.Pipelines
			.Include(p => p.Items)
			.ThenInclude(i => i.Task)
			.FirstOrDefaultAsync(p => p.Id == pipelineId);

		if (pipeline is null)
			return NotFound($"Pipeline with id [{pipelineId}] is not found");

		var task = await _appDbContext.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
		if (task is null)
			return NotFound($"Task with id [{taskId}] is not found");

		var pipelineItem = new PipelineItem
		{
			Pipeline = pipeline,
			PipelineId = pipelineId,
			Task = task,
			TaskId = taskId
		};
		pipeline.Items.Add(pipelineItem);

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

		var pipeline = await _appDbContext.Pipelines
			.Include(p => p.Items)
			.ThenInclude(i => i.Task)
			.FirstOrDefaultAsync(p => p.Id == pipelineId);

		if (pipeline is null)
			return NotFound($"Pipeline with id [{pipelineId}] is not found");

		var pipelineItem = pipeline.Items.FirstOrDefault(t => t.Task.Id == taskId);
		if (pipelineItem is null)
			return NotFound($"Task with id [{taskId}] is not found in pipeline [{pipelineId}]");

		pipeline.Items.Remove(pipelineItem);

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

	// POST: /pipelines/{id}/run
	[HttpPost("{id:int}/run")]
	public async Task<IActionResult> RunPipeline(int id)
	{
		var apiKey = Request.Headers["UserApiKey"].ToString();
		var isValid = apiKey != null && _userService.VerifyToken(apiKey);
		if (!isValid)
		{
			return Unauthorized("Invalid API token.");
		}

		var pipeline = await _appDbContext.Pipelines
			.Include(p => p.Items)
			.ThenInclude(i => i.Task)
			.FirstOrDefaultAsync(p => p.Id == id);

		if (pipeline == null)
		{
			return NotFound(new { message = $"Pipeline {id} not found." });
		}

		if (pipeline.Status == PipelineStatus.Running)
		{
			return Conflict(new { message = $"Pipeline {id} is already running." });
		}

		pipeline.Status = PipelineStatus.Running;
		await _appDbContext.SaveChangesAsync();

		var token = _pipelineManager.IssuePipelineRunCancellationToken(id);

		double actualRunTime = 0;
		try
		{
			actualRunTime = await _taskRunManager.RunSequentialAsync(pipeline.Items, token);
		}
		catch (OperationCanceledException ex)
		{
			Console.WriteLine($"Pipeline {id} was stopped: {ex.Message}");
			return Ok($"Pipeline {id} was stopped");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Pipeline run failed with error: {ex.Message}");
			pipeline.Status = PipelineStatus.Failed;
			await _appDbContext.SaveChangesAsync();
			throw;
		}

		pipeline.PipelineRunTime = actualRunTime;
		pipeline.Status = PipelineStatus.Finished;
		await _appDbContext.SaveChangesAsync();

		if (!_pipelineManager.TryUtilizePipelineRunCancellationToken(id))
		{
			Console.WriteLine($"Error utilizing the pipeline {id} cancellation token.");
		}

		return Ok($"Pipeline {id} has finished running in {actualRunTime} seconds.");
		// TODO: return accepted and subscribe to the result instead of waiting
		// return Accepted(new { message = "Pipeline run has been started", pipelineId = pipelineId, status = pipeline.Status });
	}

	// POST: /pipelines/{id}/stop
	[HttpPost("{id:int}/stop")]
	public async Task<IActionResult> StopPipeline(int id)
	{
		var apiKey = Request.Headers["UserApiKey"].ToString();
		var isValid = apiKey != null && _userService.VerifyToken(apiKey);
		if (!isValid)
		{
			return Unauthorized("Invalid API token.");
		}

		var pipeline = await _appDbContext.Pipelines
			.Include(p => p.Items)
			.ThenInclude(i => i.Task)
			.FirstOrDefaultAsync(p => p.Id == id);

		if (pipeline == null)
		{
			return NotFound(new { message = $"Pipeline {id} not found." });
		}

		if (pipeline.Status != PipelineStatus.Running)
		{
			return Conflict(new { message = $"Pipeline {id} is not running." });
		}

		try
		{
			if (!_pipelineManager.TryStopPipeline(id))
				throw new ApplicationException($"Unable to stop pipeline {id}.");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Pipeline {id} stop failed with error: {ex.Message}");
			pipeline.Status = PipelineStatus.Failed;
			await _appDbContext.SaveChangesAsync();
			throw;
		}

		pipeline.Status = PipelineStatus.Stopped;
		await _appDbContext.SaveChangesAsync();

		return Ok($"Pipeline {id} was stopped successfully."); ;
	}
}
