using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebhookGateway.API.Application.Exceptions;

namespace WebhookGateway.API.Api.Middleware;

public static class ProblemDetailsFactory
{
	public static ProblemDetails Create(Exception ex)
	{
		return ex switch
		{
			ValidationException => Create(StatusCodes.Status400BadRequest, "Validation failed", ex.Message),
			JsonException => Create(StatusCodes.Status400BadRequest, "Validation failed", ex.Message),
			NotSupportedException => Create(StatusCodes.Status400BadRequest, "Validation failed", ex.Message),
			NotFoundException => Create(StatusCodes.Status404NotFound, "Resource not found", "The requested resource was not found."),
			_ => Create(StatusCodes.Status500InternalServerError, "Internal server error", "An unexpected error occurred.")
		};
	}

	private static ProblemDetails Create(int status, string title, string detail)
	{
		return new ProblemDetails
		{
			Status = status,
			Title = title,
			Detail = detail,
		};
	}
}