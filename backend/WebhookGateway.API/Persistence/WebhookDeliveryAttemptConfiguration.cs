using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Persistence;

public class WebhookDeliveryAttemptConfiguration : IEntityTypeConfiguration<WebhookDeliveryAttempt>
{
	public void Configure(EntityTypeBuilder<WebhookDeliveryAttempt> builder)
	{
		builder.HasKey(x => x.Id);
		
		builder.HasIndex(x => x.WebhookDeliveryId);

		builder.Property(x => x.Status)
			.HasConversion<string>();
		
		builder.Property(x => x.ErrorMessage)
			.HasMaxLength(10000);
	}
}