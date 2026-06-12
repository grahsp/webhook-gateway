using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebhookGateway.API.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WebhookRoutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WebhookEvents_Source_DeliveryId",
                table: "WebhookEvents");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "WebhookEvents");

            migrationBuilder.AlterColumn<string>(
                name: "Payload",
                table: "WebhookEvents",
                type: "character varying(100000)",
                maxLength: 100000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "WebhookRouteId",
                table: "WebhookEvents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "WebhookRoutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookRoutes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WebhookEvents_WebhookRouteId_DeliveryId",
                table: "WebhookEvents",
                columns: new[] { "WebhookRouteId", "DeliveryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebhookRoutes_Source",
                table: "WebhookRoutes",
                column: "Source");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebhookRoutes");

            migrationBuilder.DropIndex(
                name: "IX_WebhookEvents_WebhookRouteId_DeliveryId",
                table: "WebhookEvents");

            migrationBuilder.DropColumn(
                name: "WebhookRouteId",
                table: "WebhookEvents");

            migrationBuilder.AlterColumn<string>(
                name: "Payload",
                table: "WebhookEvents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100000)",
                oldMaxLength: 100000);

            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "WebhookEvents",
                type: "integer",
                maxLength: 50,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WebhookEvents_Source_DeliveryId",
                table: "WebhookEvents",
                columns: new[] { "Source", "DeliveryId" },
                unique: true);
        }
    }
}
