using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lihtar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToMenuCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MenuCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MenuCategories");
        }
    }
}
