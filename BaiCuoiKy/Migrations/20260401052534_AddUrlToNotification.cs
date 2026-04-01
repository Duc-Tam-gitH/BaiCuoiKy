using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaiCuoiKy.Migrations
{
    /// <inheritdoc />
    public partial class AddUrlToNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "Notifications");
        }
    }
}
