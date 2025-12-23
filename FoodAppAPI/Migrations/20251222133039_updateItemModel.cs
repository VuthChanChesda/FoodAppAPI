using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodAppAPI.Migrations
{
    /// <inheritdoc />
    public partial class updateItemModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsExpiredProcessed",
                table: "Items",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsExpiredProcessed",
                table: "Items");
        }
    }
}
