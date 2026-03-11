using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWallet.Domain.Entities;

namespace MyWallet.Infrastructure.Persistence.DbConfigurations
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

            builder.Property(a => a.BankName)
                .IsRequired(false)
                .HasMaxLength(255);

            builder.Property(qr => qr.Provider)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

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

            builder.Property(qr => qr.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Indexes
            // Uniqueness: 1 user không được trùng AccountNumber
            builder.HasIndex(a => new { a.UserId, a.AccountNumber, a.BankCode, a.Provider })
                .IsUnique()
                .HasDatabaseName("IX_Accounts_UserId_AccountNumber_BankCode_Provider");

            // Truy vấn phổ biến: lấy danh sách account theo user (covering index)
            builder.HasIndex(a => new { a.UserId, a.IsPinned, a.CreatedAt })
                .HasDatabaseName("IX_Accounts_UserId_IsPinned_CreatedAt")
                .IncludeProperties(a => new
                {
                    a.AccountNumber,
                    a.AccountHolder,
                    a.BankCode,
                    a.BankName,
                    a.Provider,
                    a.Balance,
                    a.IsActive
                });

            // Truy vấn lọc account đang active theo user
            builder.HasIndex(a => new { a.UserId, a.IsActive })
                .HasDatabaseName("IX_Accounts_UserId_IsActive")
                .IncludeProperties(a => new
                {
                    a.AccountNumber,
                    a.AccountHolder,
                    a.BankCode,
                    a.BankName,
                    a.Provider,
                    a.Balance,
                    a.IsPinned,
                    a.CreatedAt
                });

            builder.HasIndex(a => a.BankCode)
                .HasDatabaseName("IX_Accounts_BankCode")
                .IncludeProperties(a => new { a.BankName });

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
