using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskPipeline.ApiService.Models;

namespace TaskPipeline.ApiService
{
	[ApiController]
	[Route("pipelines")]
	public class PipelineController : ControllerBase
	{
		private readonly PipelineDbContext _db;
		private readonly UserService _userService;

		public PipelineController(PipelineDbContext db, UserService userService)
		{
			_db = db ?? throw new ArgumentNullException(nameof(db));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
		}

		// GET: /pipelines
		[HttpGet]
		public async Task<IActionResult> GetAllPipelines()
		{
			var pipelines = await _db.Pipelines.Include(p => p.Tasks).ToListAsync();
			return Ok(pipelines);
		}

		// GET: /pipelines/{id}
		[HttpGet("{id:int}")]
		public async Task<IActionResult> GetPipelineById(int id)
		{
			var pipeline = await _db.Pipelines.Include(p => p.Tasks).FirstOrDefaultAsync(p => p.Id == id);
			return pipeline is not null ? Ok(pipeline) : NotFound();
		}

		// GET: /pipelines/{id}/time
		[HttpGet("{id:int}/time")]
		public async Task<IActionResult> GetPipelineTotalTimeById(int id)
		{
			var pipeline = await _db.Pipelines.Include(p => p.Tasks).FirstOrDefaultAsync(p => p.Id == id);
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
			_db.Pipelines.Add(pipeline);
			await _db.SaveChangesAsync();
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

			var pipeline = await _db.Pipelines.Include(p => p.Tasks).FirstOrDefaultAsync(p => p.Id == id);
			if (pipeline is null)
				return NotFound();

			pipeline.Name = updatedPipeline.Name;
			pipeline.Tasks = updatedPipeline.Tasks;
			pipeline.Description = updatedPipeline.Description;
			pipeline.Status = updatedPipeline.Status;

			await _db.SaveChangesAsync();
			return NoContent();
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

			var pipeline = await _db.Pipelines.FindAsync(id);
			if (pipeline is null)
				return NotFound();

			_db.Pipelines.Remove(pipeline);
			await _db.SaveChangesAsync();
			return NoContent();
		}
	}
}
