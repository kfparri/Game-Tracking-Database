using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class game_object_property_name_changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Category",
                table: "Games",
                newName: "GameType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GameType",
                table: "Games",
                newName: "Category");
        }
    }
}
