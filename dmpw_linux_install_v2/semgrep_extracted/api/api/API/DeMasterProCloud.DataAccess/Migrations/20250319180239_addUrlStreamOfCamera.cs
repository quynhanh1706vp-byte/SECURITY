using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeMasterProCloud.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addUrlStreamOfCamera : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UrlStream",
                table: "Camera",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VmsUrlStream",
                table: "Camera",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Camera_UrlStream",
                table: "Camera",
                column: "UrlStream");

            migrationBuilder.CreateIndex(
                name: "IX_Camera_VmsUrlStream",
                table: "Camera",
                column: "VmsUrlStream");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Camera_UrlStream",
                table: "Camera");

            migrationBuilder.DropIndex(
                name: "IX_Camera_VmsUrlStream",
                table: "Camera");

            migrationBuilder.DropColumn(
                name: "UrlStream",
                table: "Camera");

            migrationBuilder.DropColumn(
                name: "VmsUrlStream",
                table: "Camera");
        }
    }
}
