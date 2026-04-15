using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InnSales.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class testmigcration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BasketItems_Pps_Promocode_PromoCodeId",
                table: "BasketItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Pps_Promocode_PromoCodeId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Pps_Promocode_Pps_Promotion_PromotionId",
                table: "Pps_Promocode");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pps_Promotion",
                table: "Pps_Promotion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pps_Promocode",
                table: "Pps_Promocode");

            migrationBuilder.RenameTable(
                name: "Pps_Promotion",
                newName: "InnSales_Promotion");

            migrationBuilder.RenameTable(
                name: "Pps_Promocode",
                newName: "InnSales_Promocode");

            migrationBuilder.RenameIndex(
                name: "IX_Pps_Promocode_PromotionId",
                table: "InnSales_Promocode",
                newName: "IX_InnSales_Promocode_PromotionId");

            migrationBuilder.RenameIndex(
                name: "IX_Pps_Promocode_Code",
                table: "InnSales_Promocode",
                newName: "IX_InnSales_Promocode_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InnSales_Promotion",
                table: "InnSales_Promotion",
                column: "PromotionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InnSales_Promocode",
                table: "InnSales_Promocode",
                column: "PromoCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_BasketItems_InnSales_Promocode_PromoCodeId",
                table: "BasketItems",
                column: "PromoCodeId",
                principalTable: "InnSales_Promocode",
                principalColumn: "PromoCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_InnSales_Promocode_InnSales_Promotion_PromotionId",
                table: "InnSales_Promocode",
                column: "PromotionId",
                principalTable: "InnSales_Promotion",
                principalColumn: "PromotionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_InnSales_Promocode_PromoCodeId",
                table: "OrderItems",
                column: "PromoCodeId",
                principalTable: "InnSales_Promocode",
                principalColumn: "PromoCodeId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BasketItems_InnSales_Promocode_PromoCodeId",
                table: "BasketItems");

            migrationBuilder.DropForeignKey(
                name: "FK_InnSales_Promocode_InnSales_Promotion_PromotionId",
                table: "InnSales_Promocode");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_InnSales_Promocode_PromoCodeId",
                table: "OrderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InnSales_Promotion",
                table: "InnSales_Promotion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InnSales_Promocode",
                table: "InnSales_Promocode");

            migrationBuilder.RenameTable(
                name: "InnSales_Promotion",
                newName: "Pps_Promotion");

            migrationBuilder.RenameTable(
                name: "InnSales_Promocode",
                newName: "Pps_Promocode");

            migrationBuilder.RenameIndex(
                name: "IX_InnSales_Promocode_PromotionId",
                table: "Pps_Promocode",
                newName: "IX_Pps_Promocode_PromotionId");

            migrationBuilder.RenameIndex(
                name: "IX_InnSales_Promocode_Code",
                table: "Pps_Promocode",
                newName: "IX_Pps_Promocode_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pps_Promotion",
                table: "Pps_Promotion",
                column: "PromotionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pps_Promocode",
                table: "Pps_Promocode",
                column: "PromoCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_BasketItems_Pps_Promocode_PromoCodeId",
                table: "BasketItems",
                column: "PromoCodeId",
                principalTable: "Pps_Promocode",
                principalColumn: "PromoCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Pps_Promocode_PromoCodeId",
                table: "OrderItems",
                column: "PromoCodeId",
                principalTable: "Pps_Promocode",
                principalColumn: "PromoCodeId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Pps_Promocode_Pps_Promotion_PromotionId",
                table: "Pps_Promocode",
                column: "PromotionId",
                principalTable: "Pps_Promotion",
                principalColumn: "PromotionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
