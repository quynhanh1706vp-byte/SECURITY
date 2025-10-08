using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeMasterProCloud.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateTableAccessSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "AccessSchedule",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AccessSchedule_CompanyId",
                table: "AccessSchedule",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessSchedule_Company",
                table: "AccessSchedule",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessSchedule_Company",
                table: "AccessSchedule");

            migrationBuilder.DropIndex(
                name: "IX_AccessSchedule_CompanyId",
                table: "AccessSchedule");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "AccessSchedule");
        }
    }
}
