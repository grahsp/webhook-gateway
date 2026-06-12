using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Persistence;

public class WebhookDestinationConfiguration : IEntityTypeConfiguration<WebhookDestination>
{
	public void Configure(EntityTypeBuilder<WebhookDestination> builder)
	{
		builder.HasKey(x => x.Id);

		builder.HasIndex(x => new { x.WebhookRouteId, x.Url })
			.IsUnique();
		
		builder.Property(x => x.Url)
			.HasMaxLength(2048)
			.IsRequired();
	}
}