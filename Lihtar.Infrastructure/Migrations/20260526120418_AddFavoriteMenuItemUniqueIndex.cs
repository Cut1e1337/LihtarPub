using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lihtar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFavoriteMenuItemUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_FavoriteMenuItems_UserId_MenuItemId",
                table: "FavoriteMenuItems",
                columns: new[] { "UserId", "MenuItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FavoriteMenuItems_UserId_MenuItemId",
                table: "FavoriteMenuItems");
        }
    }
}
