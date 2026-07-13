using CocoQR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CocoQR.Infrastructure.Persistence.DbConfigurations
{
    public class CocoMailCallbackEventConfiguration
        : IEntityTypeConfiguration<CocoMailCallbackEvent>
    {
        public void Configure(EntityTypeBuilder<CocoMailCallbackEvent> builder)
        {
            builder.ToTable("CocoMailCallbackEvents");

            builder.HasKey(x => x.Id).IsClustered();
            builder.Property(x => x.EventId).IsRequired().HasMaxLength(100);
            builder.Property(x => x.CorrelationId).IsRequired().HasMaxLength(100);
            builder.Property(x => x.EventType).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Status).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Payload).IsRequired().HasColumnType("nvarchar(max)");
            builder.Property(x => x.ReceivedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(x => x.EventId)
                .IsUnique()
                .HasDatabaseName("IX_CocoMailCallbackEvents_EventId");

            builder.HasIndex(x => x.EmailId)
                .HasDatabaseName("IX_CocoMailCallbackEvents_EmailId");

            builder.HasIndex(x => x.CorrelationId)
                .HasDatabaseName("IX_CocoMailCallbackEvents_CorrelationId");
        }
    }
}
