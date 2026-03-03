using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
                .HasMaxLength(32)
                .ValueGeneratedOnAdd();

            builder.Property(qr => qr.UserId)
                .IsRequired(false);

            builder.Property(qr => qr.AccountId)
                .IsRequired(false);

            builder.Property(qr => qr.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(qr => qr.Description)
                .HasMaxLength(500);

            builder.Property(qr => qr.QRData)
                .HasMaxLength(2000);

            builder.Property(qr => qr.QRImageUrl)
                .HasMaxLength(500);

            builder.Property(qr => qr.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(qr => qr.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Indexes
            builder.HasIndex(o => new { o.UserId, o.CreatedAt })
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("IX_QRHistories_User_Paging");

            builder.HasIndex(o => new { o.AccountId, o.CreatedAt })
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("IX_QRHistories_Account_Paging");

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
