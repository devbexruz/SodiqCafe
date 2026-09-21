using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SodiqCafeMVC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserStampAndB2BBilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MonthlyFee",
                table: "Cafes",
                newName: "PricePerExtraUsers");

            migrationBuilder.AddColumn<string>(
                name: "ReceiptImageUrl",
                table: "Invoices",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalActiveUsers",
                table: "Invoices",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "Cafes",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "BaseUsersLimit",
                table: "Cafes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExtraUsersStep",
                table: "Cafes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasCustomPrice",
                table: "Cafes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CafePriceChangeHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CafeId = table.Column<int>(type: "INTEGER", nullable: false),
                    OldPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    NewPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    AdminId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CafePriceChangeHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CafePriceChangeHistories_Cafes_CafeId",
                        column: x => x.CafeId,
                        principalTable: "Cafes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CafePriceChangeHistories_Users_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserStamps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    CafeId = table.Column<int>(type: "INTEGER", nullable: false),
                    StampCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastStampDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStamps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserStamps_Cafes_CafeId",
                        column: x => x.CafeId,
                        principalTable: "Cafes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserStamps_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CafePriceChangeHistories_AdminId",
                table: "CafePriceChangeHistories",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_CafePriceChangeHistories_CafeId",
                table: "CafePriceChangeHistories",
                column: "CafeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStamps_CafeId",
                table: "UserStamps",
                column: "CafeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStamps_UserId_CafeId",
                table: "UserStamps",
                columns: new[] { "UserId", "CafeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CafePriceChangeHistories");

            migrationBuilder.DropTable(
                name: "UserStamps");

            migrationBuilder.DropColumn(
                name: "ReceiptImageUrl",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "TotalActiveUsers",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "Cafes");

            migrationBuilder.DropColumn(
                name: "BaseUsersLimit",
                table: "Cafes");

            migrationBuilder.DropColumn(
                name: "ExtraUsersStep",
                table: "Cafes");

            migrationBuilder.DropColumn(
                name: "HasCustomPrice",
                table: "Cafes");

            migrationBuilder.RenameColumn(
                name: "PricePerExtraUsers",
                table: "Cafes",
                newName: "MonthlyFee");
        }
    }
}
