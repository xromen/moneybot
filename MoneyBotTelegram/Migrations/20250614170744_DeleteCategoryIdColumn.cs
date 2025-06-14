using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyBotTelegram.Migrations
{
    /// <inheritdoc />
    public partial class DeleteCategoryIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_purchase_items_categories_category_id",
                table: "purchase_items");

            migrationBuilder.DropIndex(
                name: "ix_purchase_items_category_id",
                table: "purchase_items");

            migrationBuilder.DropColumn(
                name: "category_id",
                table: "purchase_items");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "category_id",
                table: "purchase_items",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "ix_purchase_items_category_id",
                table: "purchase_items",
                column: "category_id");

            migrationBuilder.AddForeignKey(
                name: "fk_purchase_items_categories_category_id",
                table: "purchase_items",
                column: "category_id",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
