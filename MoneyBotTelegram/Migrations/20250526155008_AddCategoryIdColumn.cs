using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyBotTelegram.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "categoryid",
                table: "transactions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "ix_transactions_categoryid",
                table: "transactions",
                column: "categoryid");

            migrationBuilder.AddForeignKey(
                name: "fk_transactions_categories_categoryid",
                table: "transactions",
                column: "categoryid",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_transactions_categories_categoryid",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "ix_transactions_categoryid",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "categoryid",
                table: "transactions");
        }
    }
}
