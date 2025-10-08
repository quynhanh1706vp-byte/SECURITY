using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeMasterProCloud.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class add_vehicle_type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VehicleType",
                table: "Vehicle",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VehicleType",
                table: "Vehicle");
        }
    }
}
