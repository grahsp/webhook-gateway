using Microsoft.EntityFrameworkCore;
using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> opts) : DbContext(opts)
{
	public DbSet<WebhookEvent> WebhookEvents { get; set; }
	
	protected override void OnModelCreating(ModelBuilder builder)
	{
		builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}
}