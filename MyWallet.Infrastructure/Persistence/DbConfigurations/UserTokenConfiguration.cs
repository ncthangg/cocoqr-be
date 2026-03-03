using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWallet.Domain.Entities;

namespace MyWallet.Infrastructure.Persistence.DbConfigurations
{
    public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
    {
        public void Configure(EntityTypeBuilder<UserToken> builder)
        {
            // Table name
            builder.ToTable("UserTokens");

            // Primary key
            builder.HasKey(a => a.Id);

            // Properties configuration
            builder.Property(a => a.UserId)
                .IsRequired();

            // Properties configuration
            builder.Property(a => a.RefreshToken)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(a => a.ExpiredTime)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // BaseEntity properties
            builder.Property(a => a.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(a => a.Status)
                .HasDefaultValue(true);

            // Indexes
            builder.HasIndex(a => a.UserId)
                .HasDatabaseName("IX_UserTokens_UserId");

            builder.HasIndex(a => a.RefreshToken)
                .HasDatabaseName("IX_UserTokens_RefreshToken");

        }
    }
}
