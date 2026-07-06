using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CocoQR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailConversations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailConversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContactMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InitiatorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecipientUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InitiatorEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RecipientEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailConversations", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "EmailConversationMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    SenderUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecipientUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FromEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ToEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GatewayMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailConversationMessages", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                    table.ForeignKey(
                        name: "FK_EmailConversationMessages_EmailConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "EmailConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailConversations_ContactMessageId",
                table: "EmailConversations",
                column: "ContactMessageId",
                unique: true,
                filter: "[ContactMessageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EmailConversations_Initiator_LastMessageAt",
                table: "EmailConversations",
                columns: new[] { "InitiatorUserId", "LastMessageAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailConversations_Recipient_LastMessageAt",
                table: "EmailConversations",
                columns: new[] { "RecipientUserId", "LastMessageAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailConversationMessages_Conversation_Sequence",
                table: "EmailConversationMessages",
                columns: new[] { "ConversationId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailConversationMessages_Recipient_CreatedAt",
                table: "EmailConversationMessages",
                columns: new[] { "RecipientUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailConversationMessages_Sender_CreatedAt",
                table: "EmailConversationMessages",
                columns: new[] { "SenderUserId", "CreatedAt" });

            migrationBuilder.Sql(
                """
                INSERT INTO EmailConversations
                (
                    Id,
                    ContactMessageId,
                    InitiatorUserId,
                    RecipientUserId,
                    InitiatorEmail,
                    RecipientEmail,
                    Subject,
                    LastMessageAt,
                    CreatedAt
                )
                SELECT
                    contact.Id,
                    contact.Id,
                    sender.Id,
                    NULL,
                    contact.Email,
                    'cocoqr',
                    'Lien he moi tu nguoi dung',
                    contact.CreatedAt,
                    contact.CreatedAt
                FROM ContactMessages contact
                LEFT JOIN Users sender ON sender.Email = contact.Email;

                INSERT INTO EmailConversationMessages
                (
                    Id,
                    ConversationId,
                    SequenceNumber,
                    SenderUserId,
                    RecipientUserId,
                    FromEmail,
                    ToEmail,
                    Subject,
                    Body,
                    Direction,
                    Status,
                    CreatedAt
                )
                SELECT
                    NEWID(),
                    contact.Id,
                    1,
                    sender.Id,
                    NULL,
                    contact.Email,
                    'cocoqr',
                    'Lien he moi tu nguoi dung',
                    contact.Content,
                    'INBOUND',
                    'RECEIVED',
                    contact.CreatedAt
                FROM ContactMessages contact
                LEFT JOIN Users sender ON sender.Email = contact.Email;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailConversationMessages");

            migrationBuilder.DropTable(
                name: "EmailConversations");
        }
    }
}
