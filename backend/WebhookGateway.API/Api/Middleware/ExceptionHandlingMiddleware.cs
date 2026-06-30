using WebhookGateway.API.Logging;

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
			logger.RequestCanceled(context.Request.Method, context.Request.Path);
		}
		catch (Exception ex)
		{
			logger.UnhandledRequestException(ex, context.Request.Method, context.Request.Path);

			if (context.Response.HasStarted)
				throw;
			
			var problem = ProblemDetailsFactory.Create(ex);

			context.Response.StatusCode = problem.Status!.Value;
			await context.Response.WriteAsJsonAsync(problem);
		}
	}
}
