using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeMasterProCloud.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateFKOfNationalIdCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_NationalIdCard",
                table: "NationalIdCard");

            migrationBuilder.DropForeignKey(
                name: "FK_Visit_NationalIdCard",
                table: "NationalIdCard");

            migrationBuilder.AddForeignKey(
                name: "FK_User_NationalIdCard",
                table: "NationalIdCard",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Visit_NationalIdCard",
                table: "NationalIdCard",
                column: "VisitId",
                principalTable: "Visit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_NationalIdCard",
                table: "NationalIdCard");

            migrationBuilder.DropForeignKey(
                name: "FK_Visit_NationalIdCard",
                table: "NationalIdCard");

            migrationBuilder.AddForeignKey(
                name: "FK_User_NationalIdCard",
                table: "NationalIdCard",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Visit_NationalIdCard",
                table: "NationalIdCard",
                column: "VisitId",
                principalTable: "Visit",
                principalColumn: "Id");
        }
    }
}
