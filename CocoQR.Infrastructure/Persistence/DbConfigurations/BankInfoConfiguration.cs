using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CocoQR.Domain.Entities;

namespace CocoQR.Infrastructure.Persistence.DbConfigurations
{
    public class BankInfoConfiguration : IEntityTypeConfiguration<BankInfo>
    {
        public void Configure(EntityTypeBuilder<BankInfo> builder)
        {
            // Table name
            builder.ToTable("BankInfos");

            // Primary key
            builder.HasKey(b => b.Id)
                .IsClustered();

            // Properties configuration
            builder.Property(b => b.BankCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(b => b.NapasBin)
                .HasMaxLength(20);

            builder.Property(b => b.SwiftCode)
                .HasMaxLength(20);

            builder.Property(b => b.BankName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(b => b.ShortName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(b => b.LogoUrl)
                .HasMaxLength(500);

            builder.Property(b => b.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // BaseEntity properties
            builder.Property(a => a.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(a => a.Status)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            builder.HasIndex(b => b.BankCode)
                .IsUnique()
                .HasDatabaseName("IX_BankInfos_BankCode");

            builder.HasIndex(b => b.NapasBin)
                .IsUnique()
                .HasDatabaseName("IX_BankInfos_NapasBin");

            // Truy vấn phổ biến: load danh sách ngân hàng active (covering index)
            builder.HasIndex(b => b.IsActive)
                .HasDatabaseName("IX_BankInfos_IsActive")
                .IncludeProperties(b => new
                {
                    b.BankCode,
                    b.ShortName,
                    b.BankName,
                    b.LogoUrl
                });

            // Nếu thường xuyên sort theo tên trong danh sách active
            builder.HasIndex(b => new { b.IsActive, b.BankName })
                .HasDatabaseName("IX_BankInfos_IsActive_BankName")
                .IncludeProperties(b => new { b.BankCode, b.ShortName, b.LogoUrl });
        }
    }
}
