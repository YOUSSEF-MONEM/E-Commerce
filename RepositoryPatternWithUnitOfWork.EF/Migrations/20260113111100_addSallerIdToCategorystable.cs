using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepositoryPatternWithUnitOfWork.EF.Migrations
{
    /// <inheritdoc />
    public partial class addSallerIdToCategorystable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SallerId",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_SallerId",
                table: "Categories",
                column: "SallerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_SallerId",
                table: "Categories",
                column: "SallerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_SallerId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_SallerId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "SallerId",
                table: "Categories");
        }
    }
}
