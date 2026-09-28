using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class publisher_id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublisherGameID",
                table: "Games",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublisherGameID",
                table: "Games");
        }
    }
}
