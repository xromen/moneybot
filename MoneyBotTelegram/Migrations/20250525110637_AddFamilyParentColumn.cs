using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyBotTelegram.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilyParentColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "family_parent_id",
                table: "users",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_family_parent_id",
                table: "users",
                column: "family_parent_id");

            migrationBuilder.AddForeignKey(
                name: "fk_users_users_family_parent_id",
                table: "users",
                column: "family_parent_id",
                principalTable: "users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_users_users_family_parent_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_family_parent_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "family_parent_id",
                table: "users");
        }
    }
}
