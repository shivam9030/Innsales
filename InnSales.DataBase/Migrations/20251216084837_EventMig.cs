using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InnSales.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class EventMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubscriptionName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebhookSecret",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebhookUrl",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscriptionName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WebhookSecret",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WebhookUrl",
                table: "Users");
        }
    }
}
