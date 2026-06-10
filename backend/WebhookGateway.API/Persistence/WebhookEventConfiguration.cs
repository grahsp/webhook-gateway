using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebhookGateway.API.Persistence;

public class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEvent>
{
	public void Configure(EntityTypeBuilder<WebhookEvent> builder)
	{
		builder.HasKey(x => x.Id);

		builder.OwnsOne(x => x.Metadata, metadata =>
		{
			metadata.HasIndex(x => new
			{
				x.Provider,
				x.DeliveryId
			}).IsUnique();
			
			metadata.Property(x => x.Provider)
				.HasMaxLength(50)
				.HasColumnName("Provider");

			metadata.Property(x => x.EventType)
				.HasMaxLength(100)
				.HasColumnName("EventType");

			metadata.Property(x => x.DeliveryId)
				.HasMaxLength(100)
				.HasColumnName("DeliveryId");
		});

		builder.Property(x => x.Payload)
			.IsRequired();
	}
}