using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Persistence;

public class WebhookDeliveryConfiguration : IEntityTypeConfiguration<WebhookDelivery>
{
	public void Configure(EntityTypeBuilder<WebhookDelivery> builder)
	{
		builder.HasKey(x => x.Id);

		builder.HasIndex(x => x.WebhookEventId);
		
		builder.Property(x => x.Status)
			.HasConversion<string>();

		builder.HasOne(x => x.WebhookDestination)
			.WithMany()
			.HasForeignKey(x => x.WebhookDestinationId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}