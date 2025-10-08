using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DeMasterProCloud.DataAccess.Migrations
{
    public partial class Add_CreatedUpdated_DepartmentAccessGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "DepartmentAccessGroup",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "DepartmentAccessGroup",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "DepartmentAccessGroup",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "DepartmentAccessGroup",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "DepartmentAccessGroup");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "DepartmentAccessGroup");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "DepartmentAccessGroup");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "DepartmentAccessGroup");
        }
    }
}
