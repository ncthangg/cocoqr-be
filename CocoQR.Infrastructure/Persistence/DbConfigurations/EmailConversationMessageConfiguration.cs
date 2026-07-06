using CocoQR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CocoQR.Infrastructure.Persistence.DbConfigurations
{
    public class EmailConversationMessageConfiguration
        : IEntityTypeConfiguration<EmailConversationMessage>
    {
        public void Configure(EntityTypeBuilder<EmailConversationMessage> builder)
        {
            builder.ToTable("EmailConversationMessages");

            builder.HasKey(x => x.Id).IsClustered();
            builder.Property(x => x.FromEmail).IsRequired().HasMaxLength(200);
            builder.Property(x => x.ToEmail).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Subject).IsRequired().HasMaxLength(300);
            builder.Property(x => x.Body).IsRequired().HasColumnType("nvarchar(max)");
            builder.Property(x => x.Direction).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
            builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(x => new { x.ConversationId, x.SequenceNumber })
                .IsUnique()
                .HasDatabaseName("IX_EmailConversationMessages_Conversation_Sequence");

            builder.HasIndex(x => new { x.SenderUserId, x.CreatedAt })
                .HasDatabaseName("IX_EmailConversationMessages_Sender_CreatedAt");

            builder.HasIndex(x => new { x.RecipientUserId, x.CreatedAt })
                .HasDatabaseName("IX_EmailConversationMessages_Recipient_CreatedAt");

            builder.HasOne(x => x.Conversation)
                .WithMany()
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
