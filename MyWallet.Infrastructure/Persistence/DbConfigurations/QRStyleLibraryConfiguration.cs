using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWallet.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWallet.Infrastructure.Persistence.DbConfigurations
{
    public class QRStyleLibraryConfiguration : IEntityTypeConfiguration<QRStyleLibrary>
    {
        public void Configure(EntityTypeBuilder<QRStyleLibrary> builder)
        {
            builder.ToTable("QRStyleLibraries");

            // PK
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(qr => qr.UserId)
                .IsRequired(false);

            // Name
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            // JSON
            builder.Property(x => x.StyleJson)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            // Enum
            builder.Property(x => x.Type)
                .IsRequired()
                .HasConversion<string>() // lưu dạng string cho dễ đọc DB
                .HasMaxLength(20);

            builder.Property(x => x.IsDefault)
                .HasDefaultValue(false);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            // FK → User (nullable vì có system style)
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.Type);
            builder.HasIndex(x => new { x.UserId, x.Type });

            // Unique: mỗi user chỉ có 1 default style
            builder.HasIndex(x => new { x.UserId, x.IsDefault })
                .HasFilter("[IsDefault] = 1")
                .IsUnique();
        }
    }
}
