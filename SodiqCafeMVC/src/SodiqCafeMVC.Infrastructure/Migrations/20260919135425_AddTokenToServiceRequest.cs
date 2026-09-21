using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SodiqCafeMVC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenToServiceRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "ServiceRequests",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Token",
                table: "ServiceRequests");
        }
    }
}
