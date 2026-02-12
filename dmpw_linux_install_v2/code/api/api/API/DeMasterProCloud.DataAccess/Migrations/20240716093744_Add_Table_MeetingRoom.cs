using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DeMasterProCloud.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Add_Table_MeetingRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoorIds",
                table: "Meeting");

            migrationBuilder.AddColumn<int>(
                name: "MeetingRoomId",
                table: "Meeting",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MeetingRoom",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoomName = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    DoorIds = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingRoom", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingRoom_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Meeting_MeetingRoomId",
                table: "Meeting",
                column: "MeetingRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingRoom_CompanyId",
                table: "MeetingRoom",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingRoom_Id",
                table: "MeetingRoom",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingRoom_RoomName",
                table: "MeetingRoom",
                column: "RoomName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Meeting_MeetingRoom",
                table: "Meeting",
                column: "MeetingRoomId",
                principalTable: "MeetingRoom",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meeting_MeetingRoom",
                table: "Meeting");

            migrationBuilder.DropTable(
                name: "MeetingRoom");

            migrationBuilder.DropIndex(
                name: "IX_Meeting_MeetingRoomId",
                table: "Meeting");

            migrationBuilder.DropColumn(
                name: "MeetingRoomId",
                table: "Meeting");

            migrationBuilder.AddColumn<string>(
                name: "DoorIds",
                table: "Meeting",
                type: "text",
                nullable: true);
        }
    }
}
