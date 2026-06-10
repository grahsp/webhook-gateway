using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace WebhookGateway.API.Infrastructure.Extensions;

public static class PostgresExceptionExtensions
{
	public static bool IsUniqueConstraintViolation(this DbUpdateException ex)
		=> ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}