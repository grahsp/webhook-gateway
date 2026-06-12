using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebhookGateway.API.Domain;

namespace WebhookGateway.API.Persistence;

public class WebhookRouteConfiguration : IEntityTypeConfiguration<WebhookRoute>
{
	public void Configure(EntityTypeBuilder<WebhookRoute> builder)
	{
		builder.HasKey(x => x.Id);

		builder.HasIndex(x => x.Source);
		
		builder.Property(x => x.Name)
			.HasMaxLength(100)
			.IsRequired();
	}
}