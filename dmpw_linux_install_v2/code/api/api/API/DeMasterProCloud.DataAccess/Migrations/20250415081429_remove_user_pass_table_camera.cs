using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeMasterProCloud.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class remove_user_pass_table_camera : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Camera_PassWord",
                table: "Camera");

            migrationBuilder.DropIndex(
                name: "IX_Camera_UserName",
                table: "Camera");

            migrationBuilder.DropColumn(
                name: "PassWord",
                table: "Camera");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Camera");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PassWord",
                table: "Camera",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Camera",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Camera_PassWord",
                table: "Camera",
                column: "PassWord");

            migrationBuilder.CreateIndex(
                name: "IX_Camera_UserName",
                table: "Camera",
                column: "UserName");
        }
    }
}
