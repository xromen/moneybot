using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyBotTelegram.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckQrCodeRawColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "check_qr_code_raw",
                table: "money_transactions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "check_qr_code_raw",
                table: "money_transactions");
        }
    }
}
