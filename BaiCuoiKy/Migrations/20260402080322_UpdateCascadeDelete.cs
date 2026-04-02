using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaiCuoiKy.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tros_Categories_CategoryId",
                table: "Tros");

            migrationBuilder.AddForeignKey(
                name: "FK_Tros_Categories_CategoryId",
                table: "Tros",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tros_Categories_CategoryId",
                table: "Tros");

            migrationBuilder.AddForeignKey(
                name: "FK_Tros_Categories_CategoryId",
                table: "Tros",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
