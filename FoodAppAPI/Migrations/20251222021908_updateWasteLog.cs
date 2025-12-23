using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodAppAPI.Migrations
{
    /// <inheritdoc />
    public partial class updateWasteLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WasteLog_Items_ItemId",
                table: "WasteLog");

            migrationBuilder.DropForeignKey(
                name: "FK_WasteLog_Users_UserId",
                table: "WasteLog");

            migrationBuilder.DropForeignKey(
                name: "FK_WasteLog_Users_UserId1",
                table: "WasteLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WasteLog",
                table: "WasteLog");

            migrationBuilder.RenameTable(
                name: "WasteLog",
                newName: "WasteLogs");

            migrationBuilder.RenameIndex(
                name: "IX_WasteLog_UserId1",
                table: "WasteLogs",
                newName: "IX_WasteLogs_UserId1");

            migrationBuilder.RenameIndex(
                name: "IX_WasteLog_UserId",
                table: "WasteLogs",
                newName: "IX_WasteLogs_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_WasteLog_ItemId",
                table: "WasteLogs",
                newName: "IX_WasteLogs_ItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WasteLogs",
                table: "WasteLogs",
                column: "WasteLogId");

            migrationBuilder.AddForeignKey(
                name: "FK_WasteLogs_Items_ItemId",
                table: "WasteLogs",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WasteLogs_Users_UserId",
                table: "WasteLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WasteLogs_Users_UserId1",
                table: "WasteLogs",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WasteLogs_Items_ItemId",
                table: "WasteLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WasteLogs_Users_UserId",
                table: "WasteLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WasteLogs_Users_UserId1",
                table: "WasteLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WasteLogs",
                table: "WasteLogs");

            migrationBuilder.RenameTable(
                name: "WasteLogs",
                newName: "WasteLog");

            migrationBuilder.RenameIndex(
                name: "IX_WasteLogs_UserId1",
                table: "WasteLog",
                newName: "IX_WasteLog_UserId1");

            migrationBuilder.RenameIndex(
                name: "IX_WasteLogs_UserId",
                table: "WasteLog",
                newName: "IX_WasteLog_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_WasteLogs_ItemId",
                table: "WasteLog",
                newName: "IX_WasteLog_ItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WasteLog",
                table: "WasteLog",
                column: "WasteLogId");

            migrationBuilder.AddForeignKey(
                name: "FK_WasteLog_Items_ItemId",
                table: "WasteLog",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WasteLog_Users_UserId",
                table: "WasteLog",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WasteLog_Users_UserId1",
                table: "WasteLog",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
