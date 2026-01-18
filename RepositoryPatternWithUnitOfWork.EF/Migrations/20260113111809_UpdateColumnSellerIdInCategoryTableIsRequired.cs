using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepositoryPatternWithUnitOfWork.EF.Migrations
{
    /// <inheritdoc />
    public partial class UpdateColumnSellerIdInCategoryTableIsRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_SallerId",
                table: "Categories");

            migrationBuilder.AlterColumn<int>(
                name: "SallerId",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_SallerId",
                table: "Categories",
                column: "SallerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_SallerId",
                table: "Categories");

            migrationBuilder.AlterColumn<int>(
                name: "SallerId",
                table: "Categories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_SallerId",
                table: "Categories",
                column: "SallerId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
