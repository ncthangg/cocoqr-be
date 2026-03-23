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
    public class QRStyleConfiguration : IEntityTypeConfiguration<QRStyle>
    {
        public void Configure(EntityTypeBuilder<QRStyle> builder)
        {
            builder.ToTable("QRStyles");

            // PK
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.StyleJson)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            // FK → QRHistory
            builder.HasOne(x => x.QR)
                .WithMany() // nếu QRHistory chưa có navigation collection
                .HasForeignKey(x => x.QrId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index
            builder.HasIndex(x => x.QrId);

            // Optional: mỗi QR chỉ có 1 style
            builder.HasIndex(x => x.QrId)
                .IsUnique();
        }
    }
}
