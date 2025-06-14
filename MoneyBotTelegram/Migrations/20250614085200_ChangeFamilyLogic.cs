using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MoneyBotTelegram.Migrations
{
    /// <inheritdoc />
    public partial class ChangeFamilyLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_users_users_family_parent_id",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "family_parent_id",
                table: "users",
                newName: "family_id");

            migrationBuilder.RenameIndex(
                name: "ix_users_family_parent_id",
                table: "users",
                newName: "ix_users_family_id");

            migrationBuilder.CreateTable(
                name: "families",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    owner_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_families", x => x.id);
                    table.ForeignKey(
                        name: "fk_families_users_owner_id",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_families_owner_id",
                table: "families",
                column: "owner_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_users_families_family_id",
                table: "users",
                column: "family_id",
                principalTable: "families",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_users_families_family_id",
                table: "users");

            migrationBuilder.DropTable(
                name: "families");

            migrationBuilder.RenameColumn(
                name: "family_id",
                table: "users",
                newName: "family_parent_id");

            migrationBuilder.RenameIndex(
                name: "ix_users_family_id",
                table: "users",
                newName: "ix_users_family_parent_id");

            migrationBuilder.AddForeignKey(
                name: "fk_users_users_family_parent_id",
                table: "users",
                column: "family_parent_id",
                principalTable: "users",
                principalColumn: "id");
        }
    }
}
