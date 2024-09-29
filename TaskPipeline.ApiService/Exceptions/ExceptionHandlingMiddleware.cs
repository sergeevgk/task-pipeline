using System.Net;

namespace TaskPipeline.ApiService.Exceptions;

public class ExceptionHandlingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ExceptionHandlingMiddleware> _logger;

	public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An unexpected error occurred");
			await HandleExceptionAsync(context, ex);
		}
	}

	private static Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		context.Response.ContentType = "application/json";
		context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

		var response = new ErrorResponse
		{
			StatusCode = context.Response.StatusCode,
			Message = "An unexpected error occurred. Please try again later.",
			DetailedDescription = exception.Message
		};

		return context.Response.WriteAsJsonAsync(response);
	}
}

public class ErrorResponse
{
	public int StatusCode { get; set; }
	public string Message { get; set; }
	public string DetailedDescription { get; set; }
}