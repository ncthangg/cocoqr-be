using CocoQR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CocoQR.Infrastructure.Persistence.DbConfigurations
{
    public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
    {
        public void Configure(EntityTypeBuilder<EmailTemplate> builder)
        {
            builder.ToTable("EmailTemplates");

            builder.HasKey(x => x.Id)
                .IsClustered();

            builder.Property(x => x.TemplateKey)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Subject)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.Body)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.UpdatedAt)
                .IsRequired(false);

            builder.HasIndex(x => x.TemplateKey)
                .IsUnique()
                .HasDatabaseName("IX_EmailTemplates_TemplateKey");

            builder.HasIndex(x => x.IsActive)
                .HasDatabaseName("IX_EmailTemplates_IsActive");
        }
    }
}
