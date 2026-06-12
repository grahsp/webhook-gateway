using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebhookGateway.API.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WebhookDestinations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WebhookDestinations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WebhookRouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookDestinations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookDestinations_WebhookRoutes_WebhookRouteId",
                        column: x => x.WebhookRouteId,
                        principalTable: "WebhookRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDestinations_WebhookRouteId_Url",
                table: "WebhookDestinations",
                columns: new[] { "WebhookRouteId", "Url" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebhookDestinations");
        }
    }
}
