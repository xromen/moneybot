using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyBotTelegram.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "money_transactions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "money_transactions");
        }
    }
}
