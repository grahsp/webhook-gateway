namespace WebhookGateway.API.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
	RequestDelegate next,
	ILogger<ExceptionHandlingMiddleware> logger)
{
	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await next(context);
		}
		catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
		{
			logger.LogInformation("Request was canceled");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Unhandled exception");

			if (context.Response.HasStarted)
				throw;
			
			var problem = ProblemDetailsFactory.Create(ex);

			context.Response.StatusCode = problem.Status!.Value;
			await context.Response.WriteAsJsonAsync(problem);
		}
	}
}