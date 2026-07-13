using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CocoQR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCocoMailCallbackTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "EmailConversationMessages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FailureCode",
                table: "EmailConversationMessages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCallbackAt",
                table: "EmailConversationMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastCallbackEventId",
                table: "EmailConversationMessages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastCallbackPayload",
                table: "EmailConversationMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderMessageId",
                table: "EmailConversationMessages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CocoMailCallbackEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CocoMailCallbackEvents", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailConversationMessages_CorrelationId",
                table: "EmailConversationMessages",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailConversationMessages_GatewayMessageId",
                table: "EmailConversationMessages",
                column: "GatewayMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_CocoMailCallbackEvents_CorrelationId",
                table: "CocoMailCallbackEvents",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_CocoMailCallbackEvents_EmailId",
                table: "CocoMailCallbackEvents",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_CocoMailCallbackEvents_EventId",
                table: "CocoMailCallbackEvents",
                column: "EventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CocoMailCallbackEvents");

            migrationBuilder.DropIndex(
                name: "IX_EmailConversationMessages_CorrelationId",
                table: "EmailConversationMessages");

            migrationBuilder.DropIndex(
                name: "IX_EmailConversationMessages_GatewayMessageId",
                table: "EmailConversationMessages");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "EmailConversationMessages");

            migrationBuilder.DropColumn(
                name: "FailureCode",
                table: "EmailConversationMessages");

            migrationBuilder.DropColumn(
                name: "LastCallbackAt",
                table: "EmailConversationMessages");

            migrationBuilder.DropColumn(
                name: "LastCallbackEventId",
                table: "EmailConversationMessages");

            migrationBuilder.DropColumn(
                name: "LastCallbackPayload",
                table: "EmailConversationMessages");

            migrationBuilder.DropColumn(
                name: "ProviderMessageId",
                table: "EmailConversationMessages");
        }
    }
}
