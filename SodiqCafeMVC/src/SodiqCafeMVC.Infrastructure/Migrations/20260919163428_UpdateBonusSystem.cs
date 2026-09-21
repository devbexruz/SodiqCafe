using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SodiqCafeMVC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBonusSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserRewards",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<bool>(
                name: "IsTransferredToBot",
                table: "UserRewards",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TransferToken",
                table: "UserRewards",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelegramBotToken",
                table: "Cafes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelegramBotUsername",
                table: "Cafes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DailySnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CafeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalChecks = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalBonusesGiven = table.Column<int>(type: "INTEGER", nullable: false),
                    BonusesJson = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailySnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailySnapshots_Cafes_CafeId",
                        column: x => x.CafeId,
                        principalTable: "Cafes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailySnapshots_CafeId",
                table: "DailySnapshots",
                column: "CafeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailySnapshots");

            migrationBuilder.DropColumn(
                name: "IsTransferredToBot",
                table: "UserRewards");

            migrationBuilder.DropColumn(
                name: "TransferToken",
                table: "UserRewards");

            migrationBuilder.DropColumn(
                name: "TelegramBotToken",
                table: "Cafes");

            migrationBuilder.DropColumn(
                name: "TelegramBotUsername",
                table: "Cafes");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserRewards",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
