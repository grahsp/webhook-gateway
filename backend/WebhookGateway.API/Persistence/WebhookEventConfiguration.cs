using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Persistence;

public class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEvent>
{
	public void Configure(EntityTypeBuilder<WebhookEvent> builder)
	{
		builder.HasKey(x => x.Id);
		
		builder.HasIndex(x => new {
				WebhookId = x.WebhookRouteId, x.DeliveryId })
			.IsUnique();

		builder.Property(x => x.EventType)
			.HasMaxLength(100);

		builder.Property(x => x.DeliveryId)
			.HasMaxLength(100);

		builder.Property(x => x.Payload)
			.HasMaxLength(100000)
			.IsRequired();
		
		
		builder.Navigation(x => x.Deliveries)
			.UsePropertyAccessMode(PropertyAccessMode.Field);

		builder.HasMany(x => x.Deliveries)
			.WithOne()
			.HasForeignKey(x => x.WebhookEventId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}