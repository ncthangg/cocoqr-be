using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWallet.Domain.Constants.Enum;
using MyWallet.Domain.Entities;

namespace MyWallet.Infrastructure.Persistence.DbConfigurations
{
    public class QRHistoryConfiguration : IEntityTypeConfiguration<QRHistory>
    {
        public void Configure(EntityTypeBuilder<QRHistory> builder)
        {
            // Table name
            builder.ToTable("QRHistories");

            // Primary key
            builder.HasKey(x => x.Id);

            // Properties configuration
            builder.Property(qr => qr.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(qr => qr.UserId)
                .IsRequired(false);

            builder.Property(qr => qr.AccountId)
                .IsRequired(false);

            builder.Property(qr => qr.AccountNumberSnapshot)
                .HasMaxLength(50);
            builder.Property(qr => qr.AccountHolderSnapshot)
                .HasMaxLength(255);

            builder.Property(qr => qr.BankCodeSnapshot)
                .HasMaxLength(20);
            builder.Property(qr => qr.BankNameSnapshot)
                .HasMaxLength(255);
            builder.Property(qr => qr.BankShortNameSnapshot)
                .HasMaxLength(50);
            builder.Property(qr => qr.NapasBinSnapshot)
                .HasMaxLength(20);

            builder.Property(qr => qr.Amount)
                .IsRequired(false)
                .HasColumnType("decimal(18,2)");

            builder.Property(qr => qr.Currency)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(3)
                .HasDefaultValue(Currency.VND);

            builder.Property(qr => qr.Description)
                .HasMaxLength(500);

            builder.Property(qr => qr.QrData)
                .HasMaxLength(2000);

            builder.Property(qr => qr.TransactionRef)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(qr => qr.ProviderId)
                .IsRequired();

            builder.Property(qr => qr.ReceiverType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(qr => qr.IsFixedAmount)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.QrMode)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(qr => qr.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(QRStatus.CREATED);

            builder.Property(qr => qr.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(qr => qr.ExpiredAt)
                .IsRequired(false);

            builder.Property(qr => qr.PaidAt)
                .IsRequired(false);

            builder.Property(qr => qr.DeletedAt)
                .IsRequired(false);

            // Indexes
            builder.HasIndex(o => new { o.UserId, o.CreatedAt })
                .HasDatabaseName("IX_QRHistories_User_Paging");

            builder.HasIndex(o => new { o.AccountId, o.CreatedAt })
                .HasDatabaseName("IX_QRHistories_Account_Paging");

            builder.HasIndex(qr => qr.TransactionRef)
                .IsUnique()
                .HasDatabaseName("IX_QRHistory_TransactionRef");

            // Relationships
            builder.HasOne(qr => qr.User)
                .WithMany()
                .HasForeignKey(qr => qr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(qr => qr.Account)
                .WithMany(a => a.QRHistories)
                .HasForeignKey(qr => qr.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
