using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class GameSourceProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PhysicalCopy",
                table: "Games",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PurchasedFrom",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhysicalCopy",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "PurchasedFrom",
                table: "Games");
        }
    }
}
