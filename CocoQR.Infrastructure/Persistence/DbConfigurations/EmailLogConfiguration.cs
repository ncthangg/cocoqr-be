using CocoQR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CocoQR.Infrastructure.Persistence.DbConfigurations
{
    public class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
    {
        public void Configure(EntityTypeBuilder<EmailLog> builder)
        {
            builder.ToTable("EmailLogs");

            builder.HasKey(x => x.Id)
                .IsClustered();

            builder.Property(x => x.ToEmail)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Subject)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.Body)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(x => x.SmtpType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(x => x.EmailDirection)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(x => x.TemplateKey)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(x => x.ErrorMessage)
                .IsRequired(false)
                .HasMaxLength(2000);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(x => x.CreatedAt)
                .HasDatabaseName("IX_EmailLogs_CreatedAt");

            builder.HasIndex(x => new { x.SmtpType, x.CreatedAt })
                .HasDatabaseName("IX_EmailLogs_SmtpType_CreatedAt");

            builder.HasIndex(x => new { x.EmailDirection, x.CreatedAt })
                .HasDatabaseName("IX_EmailLogs_EmailDirection_CreatedAt");

            builder.HasIndex(x => new { x.ToEmail, x.CreatedAt })
                .HasDatabaseName("IX_EmailLogs_ToEmail_CreatedAt");
        }
    }
}
