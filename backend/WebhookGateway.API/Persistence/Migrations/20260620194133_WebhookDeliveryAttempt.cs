using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebhookGateway.API.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WebhookDeliveryAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "WebhookDeliveries");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "WebhookDeliveries");

            migrationBuilder.DropColumn(
                name: "FailedAt",
                table: "WebhookDeliveries");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "WebhookDeliveries");

            migrationBuilder.DropColumn(
                name: "StatusCode",
                table: "WebhookDeliveries");

            migrationBuilder.CreateTable(
                name: "WebhookDeliveryAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WebhookDeliveryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FinishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookDeliveryAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookDeliveryAttempts_WebhookDeliveries_WebhookDeliveryId",
                        column: x => x.WebhookDeliveryId,
                        principalTable: "WebhookDeliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveryAttempts_WebhookDeliveryId",
                table: "WebhookDeliveryAttempts",
                column: "WebhookDeliveryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebhookDeliveryAttempts");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeliveredAt",
                table: "WebhookDeliveries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "WebhookDeliveries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FailedAt",
                table: "WebhookDeliveries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartedAt",
                table: "WebhookDeliveries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusCode",
                table: "WebhookDeliveries",
                type: "integer",
                nullable: true);
        }
    }
}
