using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeMasterProCloud.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Remove_required_name_Meeintg_MeetingRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MeetingRoom_RoomName",
                table: "MeetingRoom");

            migrationBuilder.DropIndex(
                name: "IX_Meeting_Name",
                table: "Meeting");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MeetingRoom_RoomName",
                table: "MeetingRoom",
                column: "RoomName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Meeting_Name",
                table: "Meeting",
                column: "Name",
                unique: true);
        }
    }
}
