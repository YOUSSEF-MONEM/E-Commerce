using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepositoryPatternWithUnitOfWork.EF.Migrations
{
    /// <inheritdoc />
    public partial class UpdateColumnNameSellerIdInCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_SallerId",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "SallerId",
                table: "Categories",
                newName: "SellerId");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_SallerId",
                table: "Categories",
                newName: "IX_Categories_SellerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_SellerId",
                table: "Categories",
                column: "SellerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_SellerId",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "SellerId",
                table: "Categories",
                newName: "SallerId");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_SellerId",
                table: "Categories",
                newName: "IX_Categories_SallerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_SallerId",
                table: "Categories",
                column: "SallerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
