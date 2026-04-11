using CocoQR.Domain.Constants.Enum;
using CocoQR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CocoQR.Infrastructure.Persistence.DbConfigurations
{
    public class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
    {
        public void Configure(EntityTypeBuilder<ContactMessage> builder)
        {
            builder.ToTable("ContactMessages");

            builder.HasKey(x => x.Id)
                .IsClustered();

            builder.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Content)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(ContactMessageStatus.NEW);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.RepliedAt)
                .IsRequired(false);

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("IX_ContactMessages_Status");

            builder.HasIndex(x => new { x.Email, x.CreatedAt })
                .HasDatabaseName("IX_ContactMessages_Email_CreatedAt");
        }
    }
}
