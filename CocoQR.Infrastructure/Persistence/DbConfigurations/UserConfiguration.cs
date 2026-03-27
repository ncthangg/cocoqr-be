using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CocoQR.Domain.Entities;

namespace CocoQR.Infrastructure.Persistence.DbConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Table name
            builder.ToTable("Users");

            // Primary key
            builder.HasKey(u => u.Id).IsClustered();

            // Properties configuration
            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.GoogleId)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.PictureUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(u => u.Is2FA)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(u => u.TimeZone)
                .HasMaxLength(100);

            builder.Property(u => u.LastLoginIP)
                .HasMaxLength(50);

            builder.Property(u => u.LastLoginDevice)
                .HasMaxLength(255);

            builder.Property(u => u.SecurityStamp)
                .HasMaxLength(255);

            // BaseEntity properties
            builder.Property(a => a.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(a => a.Status)
                .IsRequired()
                .HasDefaultValue(true);

            builder.HasMany(u => u.UserTokens)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.UserRoles)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            builder.HasIndex(u => u.GoogleId)
                .IsUnique()
                .HasDatabaseName("IX_Users_GoogleId");
        }
    }
}
