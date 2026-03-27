using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CocoQR.Domain.Entities;

namespace CocoQR.Infrastructure.Persistence.DbConfigurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            // Table name
            builder.ToTable("Accounts");

            // Primary key
            builder.HasKey(a => a.Id)
                .IsClustered();

            // Properties configuration
            builder.Property(a => a.UserId)
                .IsRequired();

            builder.Property(a => a.AccountNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.AccountHolder)
                .IsRequired(false)
                .HasMaxLength(255);

            builder.Property(a => a.BankCode)
                .IsRequired(false)
                .HasMaxLength(20);

            builder.Property(qr => qr.ProviderId)
                .IsRequired();

            builder.Property(a => a.Balance)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0m);

            builder.Property(a => a.IsPinned)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(a => a.IsActive)
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
            // Uniqueness: 1 user không được trùng AccountNumber
            builder.HasIndex(a => new { a.UserId, a.AccountNumber, a.BankCode, a.ProviderId })
                .IsUnique()
                .HasDatabaseName("IX_Accounts_UserId_AccountNumber_BankCode_Provider_Unique");

            // Truy vấn phổ biến: lấy danh sách account theo user + filter
            builder.HasIndex(a => new { a.UserId, a.Status, a.DeletedAt, a.IsPinned, a.CreatedAt })
                .HasDatabaseName("IX_Accounts_UserId_Filter_Sort")
                .IncludeProperties(a => new
                {
                    a.AccountNumber,
                    a.AccountHolder,
                    a.BankCode,
                    a.Balance,
                    a.IsActive
                });

            // Truy vấn phổ biến: admin lấy danh sách account
            builder.HasIndex(a => new { a.Status, a.DeletedAt, a.CreatedAt })
                .HasDatabaseName("IX_Accounts_Status_DeletedAt_CreatedAt")
                .IncludeProperties(a => new
                {
                    a.AccountNumber,
                    a.AccountHolder,
                    a.BankCode,
                    a.Balance,
                    a.IsActive
                });

            builder.HasIndex(a => a.BankCode)
                .HasDatabaseName("IX_Accounts_BankCode");

            // Relationships
            builder.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(a => a.QRHistories)
                .WithOne(qr => qr.Account)
                .HasForeignKey(qr => qr.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
