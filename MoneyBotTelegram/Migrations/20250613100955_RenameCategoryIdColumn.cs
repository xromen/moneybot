using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyBotTelegram.Migrations
{
    /// <inheritdoc />
    public partial class RenameCategoryIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_transactions_categories_categoryid",
                table: "transactions");

            migrationBuilder.RenameColumn(
                name: "categoryid",
                table: "transactions",
                newName: "category_id");

            migrationBuilder.RenameIndex(
                name: "ix_transactions_categoryid",
                table: "transactions",
                newName: "ix_transactions_category_id");

            migrationBuilder.AddForeignKey(
                name: "fk_transactions_categories_category_id",
                table: "transactions",
                column: "category_id",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_transactions_categories_category_id",
                table: "transactions");

            migrationBuilder.RenameColumn(
                name: "category_id",
                table: "transactions",
                newName: "categoryid");

            migrationBuilder.RenameIndex(
                name: "ix_transactions_category_id",
                table: "transactions",
                newName: "ix_transactions_categoryid");

            migrationBuilder.AddForeignKey(
                name: "fk_transactions_categories_categoryid",
                table: "transactions",
                column: "categoryid",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
