using CocoQR.Domain.Constants.Enum;
using CocoQR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CocoQR.Infrastructure.Persistence.DbConfigurations
{
    public class SmtpSettingConfiguration : IEntityTypeConfiguration<SmtpSetting>
    {
        public void Configure(EntityTypeBuilder<SmtpSetting> builder)
        {
            builder.ToTable("SmtpSettings");

            builder.HasKey(x => x.Id)
                .IsClustered();

            builder.Property(x => x.Host)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Port)
                .IsRequired();

            builder.Property(x => x.Username)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Password)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.EnableSSL)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.FromEmail)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.FromName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.UpdatedAt)
                .IsRequired(false);

            builder.HasIndex(x => x.Type)
                .IsUnique()
                .HasDatabaseName("IX_SmtpSettings_Type_Unique");
        }
    }
}
