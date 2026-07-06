using CocoQR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CocoQR.Infrastructure.Persistence.DbConfigurations
{
    public class EmailConversationConfiguration : IEntityTypeConfiguration<EmailConversation>
    {
        public void Configure(EntityTypeBuilder<EmailConversation> builder)
        {
            builder.ToTable("EmailConversations");
            builder.HasKey(x => x.Id).IsClustered();

            builder.Property(x => x.InitiatorEmail).IsRequired().HasMaxLength(200);
            builder.Property(x => x.RecipientEmail).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Subject).IsRequired().HasMaxLength(300);
            builder.Property(x => x.LastMessageAt).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(x => x.ContactMessageId)
                .IsUnique()
                .HasFilter("[ContactMessageId] IS NOT NULL")
                .HasDatabaseName("IX_EmailConversations_ContactMessageId");

            builder.HasIndex(x => new { x.InitiatorUserId, x.LastMessageAt })
                .HasDatabaseName("IX_EmailConversations_Initiator_LastMessageAt");

            builder.HasIndex(x => new { x.RecipientUserId, x.LastMessageAt })
                .HasDatabaseName("IX_EmailConversations_Recipient_LastMessageAt");
        }
    }
}
