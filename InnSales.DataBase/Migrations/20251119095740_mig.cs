using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InnSales.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class mig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BasketItems_Pps_Promocode_PromoCodeId",
                table: "BasketItems");

            migrationBuilder.AddForeignKey(
                name: "FK_BasketItems_Pps_Promocode_PromoCodeId",
                table: "BasketItems",
                column: "PromoCodeId",
                principalTable: "Pps_Promocode",
                principalColumn: "PromoCodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BasketItems_Pps_Promocode_PromoCodeId",
                table: "BasketItems");

            migrationBuilder.AddForeignKey(
                name: "FK_BasketItems_Pps_Promocode_PromoCodeId",
                table: "BasketItems",
                column: "PromoCodeId",
                principalTable: "Pps_Promocode",
                principalColumn: "PromoCodeId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
