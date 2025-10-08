using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeMasterProCloud.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class update_visit_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentLabel",
                table: "Visit",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentNumber",
                table: "Visit",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocumentType",
                table: "Visit",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentLabel",
                table: "Visit");

            migrationBuilder.DropColumn(
                name: "DocumentNumber",
                table: "Visit");

            migrationBuilder.DropColumn(
                name: "DocumentType",
                table: "Visit");
        }
    }
}
