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
    public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
    {
        public void Configure(EntityTypeBuilder<Provider> builder)
        {
            // Table name
            builder.ToTable("Providers");

            // Primary key
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Code)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            // Properties
            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(b => b.LogoUrl)
                .HasMaxLength(500);

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

            // Index
            builder.HasIndex(r => r.Code)
                .IsUnique()
                .HasDatabaseName("IX_Providers_Code");
        }
    }
}
