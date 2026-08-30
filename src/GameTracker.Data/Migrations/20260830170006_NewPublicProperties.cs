using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class NewPublicProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Platform",
                table: "Games",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Publisher",
                table: "Games",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Platform",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Publisher",
                table: "Games");
        }
    }
}
