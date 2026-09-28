using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class add_tags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameTags_Games_GamesID",
                table: "GameTags");

            migrationBuilder.DropForeignKey(
                name: "FK_GameTags_Tags_TagsID",
                table: "GameTags");

            migrationBuilder.RenameColumn(
                name: "TagsID",
                table: "GameTags",
                newName: "TagId");

            migrationBuilder.RenameColumn(
                name: "GamesID",
                table: "GameTags",
                newName: "GameId");

            migrationBuilder.RenameIndex(
                name: "IX_GameTags_TagsID",
                table: "GameTags",
                newName: "IX_GameTags_TagId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_Key",
                table: "AppSettings",
                column: "Key",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GameTags_Games_GameId",
                table: "GameTags",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameTags_Tags_TagId",
                table: "GameTags",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameTags_Games_GameId",
                table: "GameTags");

            migrationBuilder.DropForeignKey(
                name: "FK_GameTags_Tags_TagId",
                table: "GameTags");

            migrationBuilder.DropIndex(
                name: "IX_AppSettings_Key",
                table: "AppSettings");

            migrationBuilder.RenameColumn(
                name: "TagId",
                table: "GameTags",
                newName: "TagsID");

            migrationBuilder.RenameColumn(
                name: "GameId",
                table: "GameTags",
                newName: "GamesID");

            migrationBuilder.RenameIndex(
                name: "IX_GameTags_TagId",
                table: "GameTags",
                newName: "IX_GameTags_TagsID");

            migrationBuilder.AddForeignKey(
                name: "FK_GameTags_Games_GamesID",
                table: "GameTags",
                column: "GamesID",
                principalTable: "Games",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameTags_Tags_TagsID",
                table: "GameTags",
                column: "TagsID",
                principalTable: "Tags",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
