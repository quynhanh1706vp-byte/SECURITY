using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeMasterProCloud.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Add_new_field_in_Meeting_MeetingRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserIds",
                table: "Meeting");

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "MeetingRoom",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LimitAttendance",
                table: "Meeting",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "UseAlarm",
                table: "Meeting",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UserMeeting",
                columns: table => new
                {
                    MeetingId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMeeting", x => new { x.MeetingId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserMeeting_Meeting",
                        column: x => x.MeetingId,
                        principalTable: "Meeting",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserMeeting_User",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitMeeting",
                columns: table => new
                {
                    MeetingId = table.Column<int>(type: "integer", nullable: false),
                    VisitId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitMeeting", x => new { x.MeetingId, x.VisitId });
                    table.ForeignKey(
                        name: "FK_VisitMeeting_Meeting",
                        column: x => x.MeetingId,
                        principalTable: "Meeting",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VisitMeeting_Visit",
                        column: x => x.VisitId,
                        principalTable: "Visit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserMeeting_MeetingId",
                table: "UserMeeting",
                column: "MeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMeeting_UserId",
                table: "UserMeeting",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitMeeting_MeetingId",
                table: "VisitMeeting",
                column: "MeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitMeeting_VisitId",
                table: "VisitMeeting",
                column: "VisitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserMeeting");

            migrationBuilder.DropTable(
                name: "VisitMeeting");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "MeetingRoom");

            migrationBuilder.DropColumn(
                name: "LimitAttendance",
                table: "Meeting");

            migrationBuilder.DropColumn(
                name: "UseAlarm",
                table: "Meeting");

            migrationBuilder.AddColumn<string>(
                name: "UserIds",
                table: "Meeting",
                type: "text",
                nullable: true);
        }
    }
}
