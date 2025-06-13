using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MoneyBotTelegram.Migrations
{
    /// <inheritdoc />
    public partial class RenameTransactionsTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_purchase_items_transactions_transaction_id",
                table: "purchase_items");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.RenameColumn(
                name: "transaction_id",
                table: "purchase_items",
                newName: "money_transaction_id");

            migrationBuilder.RenameIndex(
                name: "ix_purchase_items_transaction_id",
                table: "purchase_items",
                newName: "ix_purchase_items_money_transaction_id");

            migrationBuilder.CreateTable(
                name: "money_transactions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    place_id = table.Column<long>(type: "bigint", nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_money_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_money_transactions_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_money_transactions_places_place_id",
                        column: x => x.place_id,
                        principalTable: "places",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_money_transactions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_money_transactions_category_id",
                table: "money_transactions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_money_transactions_place_id",
                table: "money_transactions",
                column: "place_id");

            migrationBuilder.CreateIndex(
                name: "ix_money_transactions_user_id",
                table: "money_transactions",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_purchase_items_money_transactions_money_transaction_id",
                table: "purchase_items",
                column: "money_transaction_id",
                principalTable: "money_transactions",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_purchase_items_money_transactions_money_transaction_id",
                table: "purchase_items");

            migrationBuilder.DropTable(
                name: "money_transactions");

            migrationBuilder.RenameColumn(
                name: "money_transaction_id",
                table: "purchase_items",
                newName: "transaction_id");

            migrationBuilder.RenameIndex(
                name: "ix_purchase_items_money_transaction_id",
                table: "purchase_items",
                newName: "ix_purchase_items_transaction_id");

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    place_id = table.Column<long>(type: "bigint", nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_transactions_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_transactions_places_place_id",
                        column: x => x.place_id,
                        principalTable: "places",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_transactions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_transactions_category_id",
                table: "transactions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_place_id",
                table: "transactions",
                column: "place_id");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_user_id",
                table: "transactions",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_purchase_items_transactions_transaction_id",
                table: "purchase_items",
                column: "transaction_id",
                principalTable: "transactions",
                principalColumn: "id");
        }
    }
}
