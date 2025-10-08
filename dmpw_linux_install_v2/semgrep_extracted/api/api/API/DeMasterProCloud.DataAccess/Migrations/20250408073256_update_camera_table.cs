using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeMasterProCloud.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class update_camera_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LightAlarm",
                table: "Camera",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PassWord",
                table: "Camera",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Similarity",
                table: "Camera",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Camera",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "VoiceAlarm",
                table: "Camera",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Camera_PassWord",
                table: "Camera",
                column: "PassWord");

            migrationBuilder.CreateIndex(
                name: "IX_Camera_UserName",
                table: "Camera",
                column: "UserName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Camera_PassWord",
                table: "Camera");

            migrationBuilder.DropIndex(
                name: "IX_Camera_UserName",
                table: "Camera");

            migrationBuilder.DropColumn(
                name: "LightAlarm",
                table: "Camera");

            migrationBuilder.DropColumn(
                name: "PassWord",
                table: "Camera");

            migrationBuilder.DropColumn(
                name: "Similarity",
                table: "Camera");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Camera");

            migrationBuilder.DropColumn(
                name: "VoiceAlarm",
                table: "Camera");
        }
    }
}
