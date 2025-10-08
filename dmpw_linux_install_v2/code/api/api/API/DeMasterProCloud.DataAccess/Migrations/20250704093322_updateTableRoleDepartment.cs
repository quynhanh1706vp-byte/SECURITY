using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DeMasterProCloud.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateTableRoleDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalHistory_Company",
                table: "ApprovalHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_Card_UnknownPerson",
                table: "Card");

            migrationBuilder.DropForeignKey(
                name: "FK_EventLog_UnknownPerson",
                table: "EventLog");

            migrationBuilder.DropForeignKey(
                name: "FK_IcuDevice_ControllerDevice",
                table: "IcuDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_IcuDevice_Elevator_ElevatorId",
                table: "IcuDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_IcuDevice_MealServiceTime_MealServiceTimeId",
                table: "IcuDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_IcuDevice_TwoPartSystem",
                table: "IcuDevice");

            migrationBuilder.DropTable(
                name: "BookBorrowTicket");

            migrationBuilder.DropTable(
                name: "BuildingMaster");

            migrationBuilder.DropTable(
                name: "CardHistory");

            migrationBuilder.DropTable(
                name: "CardLayout");

            migrationBuilder.DropTable(
                name: "ControllerDevice");

            migrationBuilder.DropTable(
                name: "DebugMessage");

            migrationBuilder.DropTable(
                name: "DepartmentAccessGroup");

            migrationBuilder.DropTable(
                name: "DeviceMessage");

            migrationBuilder.DropTable(
                name: "ElevatorFloor");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "EventMemo");

            migrationBuilder.DropTable(
                name: "ExceptionalMeal");

            migrationBuilder.DropTable(
                name: "IssuingDevice");

            migrationBuilder.DropTable(
                name: "MailTemplate");

            migrationBuilder.DropTable(
                name: "MealEventLog");

            migrationBuilder.DropTable(
                name: "MealServiceTime");

            migrationBuilder.DropTable(
                name: "MealSetting");

            migrationBuilder.DropTable(
                name: "MessageLog");

            migrationBuilder.DropTable(
                name: "PartTime");

            migrationBuilder.DropTable(
                name: "PersonCardType");

            migrationBuilder.DropTable(
                name: "ReportProblem");

            migrationBuilder.DropTable(
                name: "SymptomCovid");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "TwoPartSystem");

            migrationBuilder.DropTable(
                name: "UnknownPerson");

            migrationBuilder.DropTable(
                name: "UserCategoryOption");

            migrationBuilder.DropTable(
                name: "UserDiscount");

            migrationBuilder.DropTable(
                name: "UserMeeting");

            migrationBuilder.DropTable(
                name: "VideoCallLog");

            migrationBuilder.DropTable(
                name: "VisitMeeting");

            migrationBuilder.DropTable(
                name: "Book");

            migrationBuilder.DropTable(
                name: "AccidentType");

            migrationBuilder.DropTable(
                name: "Elevator");

            migrationBuilder.DropTable(
                name: "CornerSetting");

            migrationBuilder.DropTable(
                name: "MealType");

            migrationBuilder.DropTable(
                name: "Credit");

            migrationBuilder.DropTable(
                name: "CategoryOption");

            migrationBuilder.DropTable(
                name: "Meeting");

            migrationBuilder.DropTable(
                name: "BookArea");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "MeetingRoom");

            migrationBuilder.DropIndex(
                name: "IX_IcuDevice_ControllerId",
                table: "IcuDevice");

            migrationBuilder.DropIndex(
                name: "IX_IcuDevice_ElevatorId",
                table: "IcuDevice");

            migrationBuilder.DropIndex(
                name: "IX_IcuDevice_MealServiceTimeId",
                table: "IcuDevice");

            migrationBuilder.DropIndex(
                name: "IX_IcuDevice_TwoPartSystemId",
                table: "IcuDevice");

            migrationBuilder.DropIndex(
                name: "IX_EventLog_UnknownPersonId",
                table: "EventLog");

            migrationBuilder.DropIndex(
                name: "IX_Card_UnknownPersonId",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "ControllerId",
                table: "IcuDevice");

            migrationBuilder.DropColumn(
                name: "ElevatorId",
                table: "IcuDevice");

            migrationBuilder.DropColumn(
                name: "MealServiceTimeId",
                table: "IcuDevice");

            migrationBuilder.DropColumn(
                name: "TwoPartSystemId",
                table: "IcuDevice");

            migrationBuilder.DropColumn(
                name: "UnknownPersonId",
                table: "EventLog");

            migrationBuilder.DropColumn(
                name: "CreditId",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "UnknownPersonId",
                table: "Card");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "DynamicRole",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxNumberCheckout",
                table: "Department",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "MaxPercentCheckout",
                table: "Department",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalHistory_Company_CompanyId",
                table: "ApprovalHistory",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalHistory_Company_CompanyId",
                table: "ApprovalHistory");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "DynamicRole");

            migrationBuilder.DropColumn(
                name: "MaxNumberCheckout",
                table: "Department");

            migrationBuilder.DropColumn(
                name: "MaxPercentCheckout",
                table: "Department");

            migrationBuilder.AddColumn<int>(
                name: "ControllerId",
                table: "IcuDevice",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ElevatorId",
                table: "IcuDevice",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MealServiceTimeId",
                table: "IcuDevice",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TwoPartSystemId",
                table: "IcuDevice",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnknownPersonId",
                table: "EventLog",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreditId",
                table: "Company",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UnknownPersonId",
                table: "Card",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AccidentType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccidentType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccidentType_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookArea",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookArea", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookArea_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildingMaster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BuildingId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildingMaster_Building",
                        column: x => x.BuildingId,
                        principalTable: "Building",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BuildingMaster_User",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardLayout",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    AlignmentBack = table.Column<int>(type: "integer", nullable: false),
                    AlignmentFont = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LayoutBack = table.Column<string>(type: "text", nullable: true),
                    LayoutFont = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardLayout", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardLayout_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ParentCategoryId = table.Column<int>(type: "integer", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Category_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ControllerDevice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    ControllerAddress = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ControllerType = table.Column<short>(type: "smallint", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FirmwareVersion = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    MacAddress = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControllerDevice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControllerDevice_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CornerSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying", nullable: true),
                    Description = table.Column<string>(type: "character varying", nullable: true),
                    Name = table.Column<string>(type: "character varying", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CornerSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CornerSetting_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Credit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CurrentPoint = table.Column<double>(type: "double precision", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    TotalPoint = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Credit_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DebugMessage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DeviceAddress = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    MsgId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebugMessage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentAccessGroup",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "integer", nullable: false),
                    AccessGroupId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentAccessGroup", x => x.DepartmentId);
                    table.ForeignKey(
                        name: "FK_DepartmentAccessGroup_AccessGroup",
                        column: x => x.AccessGroupId,
                        principalTable: "AccessGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepartmentAccessGroup_Department",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceMessage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    MessageId = table.Column<int>(type: "integer", nullable: false),
                    Remark = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceMessage_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Elevator",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BuildingId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elevator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Elevator_Building",
                        column: x => x.BuildingId,
                        principalTable: "Building",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VisitId = table.Column<int>(type: "integer", nullable: true),
                    EquipmentName = table.Column<string>(type: "text", nullable: true),
                    ModelName = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    SerialNumber = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipment_Visit",
                        column: x => x.VisitId,
                        principalTable: "Visit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventMemo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventLogId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventMemo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventMemo_EventLog",
                        column: x => x.EventLogId,
                        principalTable: "EventLog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IssuingDevice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    ConnectionStatus = table.Column<short>(type: "smallint", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DeviceAddress = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    DevicePort = table.Column<int>(type: "integer", nullable: false),
                    DeviceType = table.Column<short>(type: "smallint", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssuingDevice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssuingDevice_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MailTemplate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    Body = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsEnable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Subject = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Variables = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MailTemplate_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MealEventLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventLogId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    AppliedDiscount = table.Column<int>(type: "integer", nullable: false),
                    CornerId = table.Column<int>(type: "integer", nullable: false),
                    ExceptionalMealAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    ExceptionalUserAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    IsManual = table.Column<bool>(type: "boolean", nullable: false),
                    MealCode = table.Column<int>(type: "integer", nullable: false),
                    MealType = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealEventLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealEventLog_EventLog",
                        column: x => x.EventLogId,
                        principalTable: "EventLog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MealServiceTime",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FriTime = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MonTime = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Remarks = table.Column<string>(type: "character varying", nullable: true),
                    SatTime = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SunTime = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ThuTime = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TueTime = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    WedTime = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealServiceTime", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealServiceTime_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MealType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    PopUpMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealType_company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeetingRoom",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    DoorIds = table.Column<string>(type: "text", nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true),
                    RoomName = table.Column<string>(type: "text", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "MessageLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    AccountId = table.Column<int>(type: "integer", nullable: true),
                    GroupMsgId = table.Column<string>(type: "text", nullable: true),
                    IsNotify = table.Column<bool>(type: "boolean", nullable: false),
                    IsStopped = table.Column<bool>(type: "boolean", nullable: false),
                    MsgId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PayLoad = table.Column<string>(type: "text", nullable: false),
                    ProcessId = table.Column<string>(type: "text", nullable: true),
                    ProgressIndex = table.Column<int>(type: "integer", nullable: false),
                    PublishedTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ResponseTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    Topic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageLog_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartTime",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    AccountHolder = table.Column<string>(type: "text", nullable: true),
                    AccountNumber = table.Column<string>(type: "text", nullable: true),
                    Bank = table.Column<string>(type: "text", nullable: true),
                    Contact = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    HourlyWage = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartTime", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartTime_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartTime_User",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PersonCardType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    ClassificationId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonCardType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonCardType_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportProblem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CameraId = table.Column<int>(type: "integer", nullable: true),
                    IcuDeviceId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Detail = table.Column<string>(type: "text", nullable: true),
                    Time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportProblem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportProblem_Camera",
                        column: x => x.CameraId,
                        principalTable: "Camera",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportProblem_IcuDevice",
                        column: x => x.IcuDeviceId,
                        principalTable: "IcuDevice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportProblem_User",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SymptomCovid",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VisitId = table.Column<int>(type: "integer", nullable: false),
                    Symptoms = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SymptomCovid", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SymptomCovid_Visit_VisitId",
                        column: x => x.VisitId,
                        principalTable: "Visit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TwoPartSystem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TimeFrom = table.Column<string>(type: "text", nullable: true),
                    TimeTo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwoPartSystem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnknownPerson",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Image = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnknownPerson", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnknownPerson_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDiscount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    End = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDiscount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CornerSetting_Company",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoCallLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    GroupId = table.Column<string>(type: "text", nullable: true),
                    RoomName = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    TokenDevice = table.Column<string>(type: "text", nullable: true),
                    TokenUser = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoCallLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CardHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccidentTypeId = table.Column<int>(type: "integer", nullable: true),
                    CardId = table.Column<int>(type: "integer", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    VisitId = table.Column<int>(type: "integer", nullable: true),
                    CardName = table.Column<string>(type: "text", nullable: true),
                    CardNo = table.Column<string>(type: "text", nullable: true),
                    CardRoleType = table.Column<short>(type: "smallint", nullable: false),
                    CardStatus = table.Column<short>(type: "smallint", nullable: false),
                    IssueCount = table.Column<int>(type: "integer", nullable: false),
                    ManagementNumber = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardHistory_AccidentType",
                        column: x => x.AccidentTypeId,
                        principalTable: "AccidentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CardHistory_Account",
                        column: x => x.UpdatedBy,
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CardHistory_Card",
                        column: x => x.CardId,
                        principalTable: "Card",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CardHistory_User",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CardHistory_Visit",
                        column: x => x.VisitId,
                        principalTable: "Visit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Book",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookAreaId = table.Column<int>(type: "integer", nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Author = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsbnNumber = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Book", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Book_BookArea",
                        column: x => x.BookAreaId,
                        principalTable: "BookArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Book_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CategoryOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ParentOptionId = table.Column<int>(type: "integer", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryOption_Category",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreditId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CurrentPoint = table.Column<double>(type: "double precision", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Point = table.Column<double>(type: "double precision", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transaction_Credit",
                        column: x => x.CreditId,
                        principalTable: "Credit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ElevatorFloor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ElevatorId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FloorIndex = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElevatorFloor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElevatorFloor_Elevator",
                        column: x => x.ElevatorId,
                        principalTable: "Elevator",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExceptionalMeal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromMealTypeId = table.Column<int>(type: "integer", nullable: false),
                    icuDeviceId = table.Column<int>(type: "integer", nullable: true),
                    ToMealTypeId = table.Column<int>(type: "integer", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExceptionalMeal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExceptionalMeal_IcuDevice_icuDeviceId",
                        column: x => x.icuDeviceId,
                        principalTable: "IcuDevice",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExceptionalMeal_MealType_FromMealTypeId",
                        column: x => x.FromMealTypeId,
                        principalTable: "MealType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExceptionalMeal_MealType_ToMealTypeId",
                        column: x => x.ToMealTypeId,
                        principalTable: "MealType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MealSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CornerId = table.Column<int>(type: "integer", nullable: false),
                    MealTypeId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealSetting_CornerSetting",
                        column: x => x.CornerId,
                        principalTable: "CornerSetting",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MealSetting_MealType",
                        column: x => x.MealTypeId,
                        principalTable: "MealType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Meeting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    MeetingRoomId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    InvitationLinkTemplate = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LimitAttendance = table.Column<int>(type: "integer", nullable: false),
                    LogInstruction = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    StatusInstruction = table.Column<bool>(type: "boolean", nullable: false),
                    TimeoutAllowed = table.Column<int>(type: "integer", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UseAlarm = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meeting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Meeting_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Meeting_MeetingRoom",
                        column: x => x.MeetingRoomId,
                        principalTable: "MeetingRoom",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookBorrowTicket",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookId = table.Column<int>(type: "integer", nullable: false),
                    BorrowerId = table.Column<int>(type: "integer", nullable: false),
                    BorrowDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CountOfExtend = table.Column<int>(type: "integer", nullable: false),
                    DeadlineDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    ReturnDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookBorrowTicket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookBorrowTicket_Book",
                        column: x => x.BookId,
                        principalTable: "Book",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookBorrowTicket_Borrower",
                        column: x => x.BorrowerId,
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCategoryOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryOptionId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCategoryOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCategoryOption_CategoryOption",
                        column: x => x.CategoryOptionId,
                        principalTable: "CategoryOption",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCategoryOption_User",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_IcuDevice_ControllerId",
                table: "IcuDevice",
                column: "ControllerId");

            migrationBuilder.CreateIndex(
                name: "IX_IcuDevice_ElevatorId",
                table: "IcuDevice",
                column: "ElevatorId");

            migrationBuilder.CreateIndex(
                name: "IX_IcuDevice_MealServiceTimeId",
                table: "IcuDevice",
                column: "MealServiceTimeId");

            migrationBuilder.CreateIndex(
                name: "IX_IcuDevice_TwoPartSystemId",
                table: "IcuDevice",
                column: "TwoPartSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_UnknownPersonId",
                table: "EventLog",
                column: "UnknownPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Card_UnknownPersonId",
                table: "Card",
                column: "UnknownPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_AccidentType_CompanyId",
                table: "AccidentType",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AccidentType_Id",
                table: "AccidentType",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Book_BookAreaId",
                table: "Book",
                column: "BookAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Book_CompanyId",
                table: "Book",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Book_Id",
                table: "Book",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BookArea_CompanyId",
                table: "BookArea",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_BookArea_Id",
                table: "BookArea",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BookBorrowTicket_BookId",
                table: "BookBorrowTicket",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookBorrowTicket_BorrowerId",
                table: "BookBorrowTicket",
                column: "BorrowerId");

            migrationBuilder.CreateIndex(
                name: "IX_BookBorrowTicket_Id",
                table: "BookBorrowTicket",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingMaster_BuildingId",
                table: "BuildingMaster",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingMaster_Id",
                table: "BuildingMaster",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingMaster_UserId",
                table: "BuildingMaster",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CardHistory_AccidentTypeId",
                table: "CardHistory",
                column: "AccidentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CardHistory_CardId",
                table: "CardHistory",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardHistory_Id",
                table: "CardHistory",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_CardHistory_UpdatedBy",
                table: "CardHistory",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CardHistory_UserId",
                table: "CardHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CardHistory_VisitId",
                table: "CardHistory",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_CardLayout_CompanyId",
                table: "CardLayout",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CardLayout_Id",
                table: "CardLayout",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Category_CompanyId",
                table: "Category",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryOption_CategoryId",
                table: "CategoryOption",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ControllerDevice_CompanyId",
                table: "ControllerDevice",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ControllerDevice_Id",
                table: "ControllerDevice",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_CornerSetting_CompanyId",
                table: "CornerSetting",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Credit_CompanyId",
                table: "Credit",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Credit_Id",
                table: "Credit",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_DebugMessage_Id",
                table: "DebugMessage",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAccessGroup_AccessGroupId",
                table: "DepartmentAccessGroup",
                column: "AccessGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAccessGroup_DepartmentId",
                table: "DepartmentAccessGroup",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMessage_CompanyId",
                table: "DeviceMessage",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Elevator_BuildingId",
                table: "Elevator",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_ElevatorFloor_ElevatorId",
                table: "ElevatorFloor",
                column: "ElevatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_VisitId",
                table: "Equipment",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_EventMemo_EventLogId",
                table: "EventMemo",
                column: "EventLogId");

            migrationBuilder.CreateIndex(
                name: "IX_EventMemo_Id",
                table: "EventMemo",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionalMeal_End",
                table: "ExceptionalMeal",
                column: "End");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionalMeal_FromMealTypeId",
                table: "ExceptionalMeal",
                column: "FromMealTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionalMeal_icuDeviceId",
                table: "ExceptionalMeal",
                column: "icuDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionalMeal_Id",
                table: "ExceptionalMeal",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionalMeal_Start",
                table: "ExceptionalMeal",
                column: "Start");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionalMeal_ToMealTypeId",
                table: "ExceptionalMeal",
                column: "ToMealTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_IssuingDevice_CompanyId",
                table: "IssuingDevice",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_IssuingDevice_Id",
                table: "IssuingDevice",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_MailTemplate_CompanyId",
                table: "MailTemplate",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_MailTemplate_Id",
                table: "MailTemplate",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_MealEventLog_EventLogId",
                table: "MealEventLog",
                column: "EventLogId");

            migrationBuilder.CreateIndex(
                name: "IX_MealServiceTime_CompanyId",
                table: "MealServiceTime",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_MealSetting_CornerId",
                table: "MealSetting",
                column: "CornerId");

            migrationBuilder.CreateIndex(
                name: "IX_MealSetting_MealTypeId",
                table: "MealSetting",
                column: "MealTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MealType_CompanyId",
                table: "MealType",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Meeting_CompanyId",
                table: "Meeting",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Meeting_Id",
                table: "Meeting",
                column: "Id");

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
                name: "IX_MessageLog_CompanyId",
                table: "MessageLog",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageLog_Id",
                table: "MessageLog",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PartTime_CompanyId",
                table: "PartTime",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PartTime_UserId",
                table: "PartTime",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonCardType_CompanyId",
                table: "PersonCardType",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonCardType_Id",
                table: "PersonCardType",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ReportProblem_CameraId",
                table: "ReportProblem",
                column: "CameraId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportProblem_IcuDeviceId",
                table: "ReportProblem",
                column: "IcuDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportProblem_Id",
                table: "ReportProblem",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ReportProblem_UserId",
                table: "ReportProblem",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SymptomCovid_Id",
                table: "SymptomCovid",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SymptomCovid_VisitId",
                table: "SymptomCovid",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_CreditId",
                table: "Transaction",
                column: "CreditId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Id",
                table: "Transaction",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TwoPartSystem_Id",
                table: "TwoPartSystem",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UnknownPerson_CompanyId",
                table: "UnknownPerson",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_UnknownPerson_Id",
                table: "UnknownPerson",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserCategoryOption_CategoryOptionId",
                table: "UserCategoryOption",
                column: "CategoryOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCategoryOption_UserId",
                table: "UserCategoryOption",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDiscount_UserId",
                table: "UserDiscount",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMeeting_MeetingId",
                table: "UserMeeting",
                column: "MeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMeeting_UserId",
                table: "UserMeeting",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCallLog_Id",
                table: "VideoCallLog",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_VisitMeeting_MeetingId",
                table: "VisitMeeting",
                column: "MeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitMeeting_VisitId",
                table: "VisitMeeting",
                column: "VisitId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalHistory_Company",
                table: "ApprovalHistory",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Card_UnknownPerson",
                table: "Card",
                column: "UnknownPersonId",
                principalTable: "UnknownPerson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventLog_UnknownPerson",
                table: "EventLog",
                column: "UnknownPersonId",
                principalTable: "UnknownPerson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IcuDevice_ControllerDevice",
                table: "IcuDevice",
                column: "ControllerId",
                principalTable: "ControllerDevice",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IcuDevice_Elevator_ElevatorId",
                table: "IcuDevice",
                column: "ElevatorId",
                principalTable: "Elevator",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IcuDevice_MealServiceTime_MealServiceTimeId",
                table: "IcuDevice",
                column: "MealServiceTimeId",
                principalTable: "MealServiceTime",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IcuDevice_TwoPartSystem",
                table: "IcuDevice",
                column: "TwoPartSystemId",
                principalTable: "TwoPartSystem",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
