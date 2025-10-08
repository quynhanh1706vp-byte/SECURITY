using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DeMasterProCloud.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class update_vehicle_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_Department",
                table: "Vehicle");

            migrationBuilder.DropTable(
                name: "VehicleAllocation");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "ExistBlackBox",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "PlateRFID",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "VehicleClass",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "VehicleName",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "VehicleRule",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "VehicleType",
                table: "Vehicle");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_Department_DepartmentId",
                table: "Vehicle",
                column: "DepartmentId",
                principalTable: "Department",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_Department_DepartmentId",
                table: "Vehicle");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Vehicle",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ExistBlackBox",
                table: "Vehicle",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PlateRFID",
                table: "Vehicle",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "Vehicle",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Vehicle",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VehicleClass",
                table: "Vehicle",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VehicleName",
                table: "Vehicle",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VehicleRule",
                table: "Vehicle",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VehicleType",
                table: "Vehicle",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "VehicleAllocation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    ManagerId = table.Column<int>(type: "integer", nullable: false),
                    UnitVehicleId = table.Column<int>(type: "integer", nullable: false),
                    Destination = table.Column<string>(type: "text", nullable: true),
                    DispatchType = table.Column<string>(type: "text", nullable: true),
                    DriverIds = table.Column<string>(type: "text", nullable: true),
                    EndTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Purpose = table.Column<string>(type: "text", nullable: true),
                    ServiceEndTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ServiceStartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    SupportType = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleAllocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleAllocation_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleAllocation_User",
                        column: x => x.ManagerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleAllocation_Vehicle",
                        column: x => x.UnitVehicleId,
                        principalTable: "Vehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAllocation_CompanyId",
                table: "VehicleAllocation",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAllocation_Id",
                table: "VehicleAllocation",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAllocation_ManagerId",
                table: "VehicleAllocation",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAllocation_UnitVehicleId",
                table: "VehicleAllocation",
                column: "UnitVehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_Department",
                table: "Vehicle",
                column: "DepartmentId",
                principalTable: "Department",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
