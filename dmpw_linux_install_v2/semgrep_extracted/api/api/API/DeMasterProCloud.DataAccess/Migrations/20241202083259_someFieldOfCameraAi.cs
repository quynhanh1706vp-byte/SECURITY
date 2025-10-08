using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeMasterProCloud.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class someFieldOfCameraAi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Distance",
                table: "EventLog",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LivenessScore",
                table: "EventLog",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SearchScore",
                table: "EventLog",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Distance",
                table: "EventLog");

            migrationBuilder.DropColumn(
                name: "LivenessScore",
                table: "EventLog");

            migrationBuilder.DropColumn(
                name: "SearchScore",
                table: "EventLog");
        }
    }
}
