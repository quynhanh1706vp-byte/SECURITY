using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DeMasterProCloud.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialNewVersionEF8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Contact = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpiredFrom = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpiredTo = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Logo = table.Column<byte[]>(type: "bytea", nullable: true),
                    MiniLogo = table.Column<byte[]>(type: "bytea", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Remarks = table.Column<string>(type: "character varying", nullable: true),
                    RootFlag = table.Column<bool>(type: "boolean", nullable: false),
                    AutoSyncUserData = table.Column<bool>(type: "boolean", nullable: false),
                    EventLogStorageDurationInDb = table.Column<int>(type: "integer", nullable: false, defaultValue: 24),
                    EventLogStorageDurationInFile = table.Column<int>(type: "integer", nullable: false, defaultValue: 24),
                    TimeLimitStoredImage = table.Column<int>(type: "integer", nullable: false, defaultValue: 365),
                    TimeLimitStoredVideo = table.Column<int>(type: "integer", nullable: false, defaultValue: 365),
                    UpdateAttendanceRealTime = table.Column<bool>(type: "boolean", nullable: false),
                    TimeRecheckAttendance = table.Column<int>(type: "integer", nullable: false, defaultValue: 15),
                    LimitCountOfUser = table.Column<int>(type: "integer", nullable: false),
                    EnableReCheckImageCamera = table.Column<bool>(type: "boolean", nullable: false),
                    TimeLimitCheckImageCamera = table.Column<int>(type: "integer", nullable: false),
                    CardBit = table.Column<int>(type: "integer", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    SecretCode = table.Column<string>(type: "text", nullable: true),
                    UseExpiredPW = table.Column<bool>(type: "boolean", nullable: false),
                    PwValidPeriod = table.Column<int>(type: "integer", nullable: false),
                    UseDataEncrypt = table.Column<bool>(type: "boolean", nullable: false),
                    WebsiteUrl = table.Column<string>(type: "text", nullable: true),
                    ContactWEmail = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Industries = table.Column<string>(type: "text", nullable: true),
                    Location = table.Column<string>(type: "text", nullable: true),
                    CreditId = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAttendanceOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DebugMessage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MsgId = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    DeviceAddress = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebugMessage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Event",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Culture = table.Column<int>(type: "integer", nullable: false),
                    EventNumber = table.Column<int>(type: "integer", nullable: false),
                    EventName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FirmwareVersion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Version = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    LinkFile = table.Column<string>(type: "text", nullable: false),
                    DeviceType = table.Column<short>(type: "smallint", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirmwareVersion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Status = table.Column<bool>(type: "boolean", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    ReceiveId = table.Column<int>(type: "integer", nullable: false),
                    ResourceName = table.Column<string>(type: "text", nullable: true),
                    ResourceParam = table.Column<string>(type: "text", nullable: true),
                    RelatedUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Setting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "character varying", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShortenLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullPath = table.Column<string>(type: "text", nullable: true),
                    ShortPath = table.Column<string>(type: "text", nullable: true),
                    LocationOrigin = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortenLink", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LicenseInfo = table.Column<string>(type: "text", nullable: true),
                    SecretCode = table.Column<string>(type: "text", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemInfo", x => x.Id);
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
                name: "AccessGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessGroup_AccessGroup",
                        column: x => x.ParentId,
                        principalTable: "AccessGroup",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AccessGroup_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    FirstApproverAccounts = table.Column<string>(type: "jsonb", nullable: true),
                    SecondApproverAccounts = table.Column<string>(type: "jsonb", nullable: true),
                    ApprovalStepNumber = table.Column<int>(type: "integer", nullable: false),
                    EnableAutoApproval = table.Column<bool>(type: "boolean", nullable: false),
                    AllowDeleteRecord = table.Column<bool>(type: "boolean", nullable: false),
                    AllLocationWarning = table.Column<string>(type: "text", nullable: true),
                    DeviceIdCheckIn = table.Column<int>(type: "integer", nullable: false),
                    ListFieldsEnable = table.Column<string>(type: "text", nullable: true),
                    VisibleFields = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessSetting_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessTime",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FriTime1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FriTime2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FriTime3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FriTime4 = table.Column<string>(type: "text", nullable: true),
                    HolType1Time1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HolType1Time2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HolType1Time3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HolType1Time4 = table.Column<string>(type: "text", nullable: true),
                    HolType2Time1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HolType2Time2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HolType2Time3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HolType2Time4 = table.Column<string>(type: "text", nullable: true),
                    HolType3Time1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HolType3Time2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HolType3Time3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HolType3Time4 = table.Column<string>(type: "text", nullable: true),
                    MonTime1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MonTime2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MonTime3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MonTime4 = table.Column<string>(type: "text", nullable: true),
                    Remarks = table.Column<string>(type: "character varying", nullable: true),
                    SatTime1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SatTime2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SatTime3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SatTime4 = table.Column<string>(type: "text", nullable: true),
                    SunTime1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SunTime2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SunTime3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SunTime4 = table.Column<string>(type: "text", nullable: true),
                    ThurTime1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ThurTime2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ThurTime3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ThurTime4 = table.Column<string>(type: "text", nullable: true),
                    TueTime1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TueTime2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TueTime3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TueTime4 = table.Column<string>(type: "text", nullable: true),
                    WedTime1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    WedTime2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    WedTime3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    WedTime4 = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessTime", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessTime_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccidentType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false)
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
                name: "ApprovalHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    OldStatus = table.Column<int>(type: "integer", nullable: true),
                    NewStatus = table.Column<int>(type: "integer", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    ApprovalType = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalHistory_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    ApproverAccounts = table.Column<string>(type: "jsonb", nullable: true),
                    TimeFormatId = table.Column<int>(type: "integer", nullable: false),
                    EnableNotifyCheckinLate = table.Column<bool>(type: "boolean", nullable: false),
                    InReaders = table.Column<string>(type: "jsonb", nullable: true),
                    OutReaders = table.Column<string>(type: "jsonb", nullable: true),
                    DayStartTime = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceSetting_Company",
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
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false)
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
                name: "Building",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    Location = table.Column<string>(type: "text", nullable: true),
                    TimeZone = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Building", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Building_Building",
                        column: x => x.ParentId,
                        principalTable: "Building",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Building_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardLayout",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LayoutFont = table.Column<string>(type: "text", nullable: true),
                    AlignmentFont = table.Column<int>(type: "integer", nullable: false),
                    LayoutBack = table.Column<string>(type: "text", nullable: true),
                    AlignmentBack = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false)
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
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ParentCategoryId = table.Column<int>(type: "integer", nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false)
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
                    ControllerAddress = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    ControllerType = table.Column<short>(type: "smallint", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying", nullable: true),
                    MacAddress = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    FirmwareVersion = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                    Name = table.Column<string>(type: "character varying", nullable: true),
                    Code = table.Column<string>(type: "character varying", nullable: true),
                    Description = table.Column<string>(type: "character varying", nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: false)
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
                    TotalPoint = table.Column<double>(type: "double precision", nullable: false),
                    CurrentPoint = table.Column<double>(type: "double precision", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false)
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
                name: "DeviceMessage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Remark = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                name: "DynamicRole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TypeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    PermissionList = table.Column<string>(type: "text", nullable: true),
                    EnableDepartmentLevel = table.Column<bool>(type: "boolean", nullable: false),
                    RoleSettingDefault = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Role_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Holiday",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Recursive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holiday", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Holiday_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IssuingDevice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DeviceAddress = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    DeviceType = table.Column<short>(type: "smallint", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DevicePort = table.Column<int>(type: "integer", nullable: false),
                    ConnectionStatus = table.Column<short>(type: "smallint", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false)
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
                name: "LeaveRequestSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumberDayOffYear = table.Column<int>(type: "integer", nullable: false),
                    NumberDayOffPreviousYear = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequestSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveRequestSetting_Company",
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
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: true),
                    Body = table.Column<string>(type: "text", nullable: true),
                    Variables = table.Column<string>(type: "jsonb", nullable: true),
                    IsEnable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                name: "MealServiceTime",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    MonTime = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TueTime = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WedTime = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ThuTime = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FriTime = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SatTime = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SunTime = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Remarks = table.Column<string>(type: "character varying", nullable: true)
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
                    Name = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
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
                name: "Meeting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimeoutAllowed = table.Column<int>(type: "integer", nullable: false),
                    StatusInstruction = table.Column<bool>(type: "boolean", nullable: false),
                    LogInstruction = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DoorIds = table.Column<string>(type: "text", nullable: true),
                    UserIds = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "MessageLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MsgId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Topic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PayLoad = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    GroupMsgId = table.Column<string>(type: "text", nullable: true),
                    ProcessId = table.Column<string>(type: "text", nullable: true),
                    ProgressIndex = table.Column<int>(type: "integer", nullable: false),
                    PublishedTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ResponseTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    IsStopped = table.Column<bool>(type: "boolean", nullable: false),
                    IsNotify = table.Column<bool>(type: "boolean", nullable: false),
                    AccountId = table.Column<int>(type: "integer", nullable: true)
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
                name: "PersonCardType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ClassificationId = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false)
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
                name: "PlugIn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    PlugIns = table.Column<string>(type: "jsonb", nullable: true),
                    PlugInsDescription = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlugIn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Solution_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UnknownPerson",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Image = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false)
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
                name: "UnregistedDevice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceAddress = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeviceType = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    MacAddress = table.Column<string>(type: "text", nullable: true),
                    OperationType = table.Column<int>(type: "integer", nullable: false),
                    RegisterType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnregistedDevice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnregistedDevice_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VisitHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VisitorId = table.Column<int>(type: "integer", nullable: false),
                    OldStatus = table.Column<int>(type: "integer", nullable: true),
                    NewStatus = table.Column<int>(type: "integer", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitHistory_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkingType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    WorkingDay = table.Column<string>(type: "jsonb", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CheckClockOut = table.Column<bool>(type: "boolean", nullable: false),
                    UseClockOutDevice = table.Column<bool>(type: "boolean", nullable: false),
                    WorkingHourType = table.Column<int>(type: "integer", nullable: false),
                    CheckLunchTime = table.Column<bool>(type: "boolean", nullable: false),
                    LunchTime = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkingType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkingType_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Visit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplyDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    VisitorName = table.Column<string>(type: "text", nullable: true),
                    VisitType = table.Column<string>(type: "text", nullable: true),
                    BirthDay = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    VisitorDepartment = table.Column<string>(type: "text", nullable: true),
                    VisitorEmpNumber = table.Column<string>(type: "text", nullable: true),
                    Position = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    VisiteeSite = table.Column<string>(type: "text", nullable: true),
                    VisitReason = table.Column<string>(type: "text", nullable: true),
                    VisiteeId = table.Column<int>(type: "integer", nullable: true),
                    VisiteeName = table.Column<string>(type: "text", nullable: true),
                    VisiteeDepartmentId = table.Column<int>(type: "integer", nullable: true),
                    VisiteeDepartment = table.Column<string>(type: "text", nullable: true),
                    VisiteeEmpNumber = table.Column<string>(type: "text", nullable: true),
                    LeaderId = table.Column<int>(type: "integer", nullable: true),
                    LeaderName = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    IsDecision = table.Column<bool>(type: "boolean", nullable: false),
                    VisitingCardState = table.Column<short>(type: "smallint", nullable: false),
                    ApproverId1 = table.Column<int>(type: "integer", nullable: false),
                    ApproverId2 = table.Column<int>(type: "integer", nullable: false),
                    CardId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IssueCount = table.Column<int>(type: "integer", nullable: false),
                    CardStatus = table.Column<short>(type: "smallint", nullable: false),
                    AccessGroupId = table.Column<int>(type: "integer", nullable: false),
                    RejectReason = table.Column<string>(type: "text", nullable: true),
                    ApprovDate1 = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ApprovDate2 = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    RejectorId = table.Column<string>(type: "text", nullable: true),
                    RejectDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserCode = table.Column<string>(type: "text", nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IssuerId = table.Column<int>(type: "integer", nullable: false),
                    ReclaimerId = table.Column<int>(type: "integer", nullable: false),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    ImageCardIdFont = table.Column<string>(type: "text", nullable: true),
                    ImageCardIdBack = table.Column<string>(type: "text", nullable: true),
                    NationalIdNumber = table.Column<string>(type: "text", nullable: true),
                    AllowedBelonging = table.Column<string>(type: "text", nullable: true),
                    Gender = table.Column<bool>(type: "boolean", nullable: false),
                    PlaceIssueIdNumber = table.Column<string>(type: "text", nullable: true),
                    DateIssueIdNumber = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    AliasId = table.Column<string>(type: "text", nullable: true),
                    VisitPlace = table.Column<string>(type: "text", nullable: true),
                    UnitName = table.Column<string>(type: "text", nullable: true),
                    GroupId = table.Column<string>(type: "text", nullable: true),
                    RoomNumber = table.Column<string>(type: "text", nullable: true),
                    RoomDoorCode = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visit_AccessGroup",
                        column: x => x.AccessGroupId,
                        principalTable: "AccessGroup",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Visit_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VisitSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    AccessTimeId = table.Column<int>(type: "integer", nullable: true),
                    FirstApproverAccounts = table.Column<string>(type: "jsonb", nullable: true),
                    SecondsApproverAccounts = table.Column<string>(type: "jsonb", nullable: true),
                    VisitCheckManagerAccounts = table.Column<string>(type: "text", nullable: true),
                    DefaultDoors = table.Column<string>(type: "jsonb", nullable: true),
                    AccessGroupId = table.Column<int>(type: "integer", nullable: false),
                    GroupDevices = table.Column<string>(type: "jsonb", nullable: true),
                    ApprovalStepNumber = table.Column<int>(type: "integer", nullable: false),
                    OutSide = table.Column<bool>(type: "boolean", nullable: false),
                    AllowEmployeeInvite = table.Column<bool>(type: "boolean", nullable: false),
                    EnableCaptCha = table.Column<bool>(type: "boolean", nullable: false),
                    EnableAutoApproval = table.Column<bool>(type: "boolean", nullable: false),
                    AllowDeleteRecord = table.Column<bool>(type: "boolean", nullable: false),
                    AllowEditRecord = table.Column<bool>(type: "boolean", nullable: false),
                    AllLocationWarning = table.Column<string>(type: "text", nullable: true),
                    DeviceIdCheckIn = table.Column<int>(type: "integer", nullable: false),
                    ListFieldsEnable = table.Column<string>(type: "text", nullable: true),
                    VisibleFields = table.Column<string>(type: "text", nullable: true),
                    FieldRegisterLeft = table.Column<string>(type: "text", nullable: true),
                    FieldRegisterRight = table.Column<string>(type: "text", nullable: true),
                    FieldRequired = table.Column<string>(type: "text", nullable: true),
                    AllowGetUserTarget = table.Column<bool>(type: "boolean", nullable: false),
                    PersonalInvitationLink = table.Column<string>(type: "text", nullable: true, defaultValue: "register-visit"),
                    AllowSelectDoorWhenCreateNew = table.Column<bool>(type: "boolean", nullable: false),
                    AllowSendKakao = table.Column<bool>(type: "boolean", nullable: false),
                    InsiderAutoApproved = table.Column<bool>(type: "boolean", nullable: false),
                    OnlyAccessSingleBuilding = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitSetting_AccessTime_AccessTimeId",
                        column: x => x.AccessTimeId,
                        principalTable: "AccessTime",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VisitSetting_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Book",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsbnNumber = table.Column<string>(type: "text", nullable: false),
                    Author = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    BookAreaId = table.Column<int>(type: "integer", nullable: true)
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
                name: "Elevator",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    BuildingId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
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
                name: "CategoryOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    ParentOptionId = table.Column<int>(type: "integer", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false)
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
                    Point = table.Column<double>(type: "double precision", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    CurrentPoint = table.Column<double>(type: "double precision", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreditId = table.Column<int>(type: "integer", nullable: false)
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
                name: "Account",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    DynamicRoleId = table.Column<int>(type: "integer", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Password = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RootFlag = table.Column<bool>(type: "boolean", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TimeZone = table.Column<string>(type: "text", nullable: true),
                    Language = table.Column<string>(type: "text", nullable: true),
                    PreferredSystem = table.Column<int>(type: "integer", nullable: false),
                    RefreshToken = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreateDateRefreshToken = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CurrentLoginInfo = table.Column<string>(type: "text", nullable: true),
                    UpdatePasswordOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Account_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Account_DynamicRole",
                        column: x => x.DynamicRoleId,
                        principalTable: "DynamicRole",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MealSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    CornerId = table.Column<int>(type: "integer", nullable: false),
                    MealTypeId = table.Column<int>(type: "integer", nullable: false)
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
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VisitId = table.Column<int>(type: "integer", nullable: true),
                    EquipmentName = table.Column<string>(type: "text", nullable: true),
                    ModelName = table.Column<string>(type: "text", nullable: true),
                    SerialNumber = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true)
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
                name: "SymptomCovid",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Symptoms = table.Column<string>(type: "jsonb", nullable: true),
                    VisitId = table.Column<int>(type: "integer", nullable: false)
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
                name: "ElevatorFloor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    FloorIndex = table.Column<int>(type: "integer", nullable: false),
                    ElevatorId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
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
                name: "BookBorrowTicket",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Note = table.Column<string>(type: "text", nullable: true),
                    BorrowDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DeadlineDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CountOfExtend = table.Column<int>(type: "integer", nullable: false),
                    BookId = table.Column<int>(type: "integer", nullable: false),
                    BorrowerId = table.Column<int>(type: "integer", nullable: false)
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
                name: "CompanyAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PreferredSystem = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    AccountId = table.Column<int>(type: "integer", nullable: true),
                    DynamicRoleId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyAccount_Account",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyAccount_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyAccount_DynamicRole",
                        column: x => x.DynamicRoleId,
                        principalTable: "DynamicRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataListSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    DataList = table.Column<string>(type: "text", nullable: true),
                    DataType = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataListSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Header_Account",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Header_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Department",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DepartName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DepartNo = table.Column<string>(type: "character varying", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DepartmentManagerId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Department", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Department_Account",
                        column: x => x.DepartmentManagerId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Department_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Department_Department",
                        column: x => x.ParentId,
                        principalTable: "Department",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HeaderSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    HeaderList = table.Column<string>(type: "text", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeaderSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Header_Account",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Header_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IcuDevice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceAddress = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    DeviceType = table.Column<short>(type: "smallint", nullable: false),
                    VerifyMode = table.Column<short>(type: "smallint", nullable: false),
                    BioStationMode = table.Column<short>(type: "smallint", nullable: false),
                    BackupPeriod = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    BuildingId = table.Column<int>(type: "integer", nullable: true),
                    ActiveTzId = table.Column<int>(type: "integer", nullable: true),
                    PassageTzId = table.Column<int>(type: "integer", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying", nullable: true),
                    ServerIp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ServerPort = table.Column<int>(type: "integer", maxLength: 8, nullable: false),
                    MacAddress = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    OperationType = table.Column<short>(type: "smallint", nullable: false),
                    RoleReader0 = table.Column<short>(type: "smallint", nullable: true),
                    RoleReader1 = table.Column<short>(type: "smallint", nullable: true),
                    LedReader0 = table.Column<short>(type: "smallint", nullable: true),
                    LedReader1 = table.Column<short>(type: "smallint", nullable: true),
                    BuzzerReader0 = table.Column<short>(type: "smallint", nullable: true),
                    BuzzerReader1 = table.Column<short>(type: "smallint", nullable: true),
                    UseCardReader = table.Column<int>(type: "integer", nullable: true),
                    SensorType = table.Column<short>(type: "smallint", nullable: false),
                    OpenDuration = table.Column<int>(type: "integer", nullable: true),
                    MaxOpenDuration = table.Column<int>(type: "integer", nullable: true),
                    SensorDuration = table.Column<int>(type: "integer", nullable: true),
                    SensorAlarm = table.Column<bool>(type: "boolean", nullable: false),
                    CloseReverseLockFlag = table.Column<bool>(type: "boolean", nullable: false),
                    PassbackRule = table.Column<short>(type: "smallint", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastCommunicationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreateTimeOnlineDevice = table.Column<string>(type: "text", nullable: true),
                    UpTimeOnlineDevice = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    MPRCount = table.Column<int>(type: "integer", nullable: false),
                    MPRInterval = table.Column<int>(type: "integer", nullable: false),
                    ConnectionStatus = table.Column<short>(type: "smallint", nullable: false),
                    DoorStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DoorStatusId = table.Column<int>(type: "integer", nullable: false),
                    AlarmStatus = table.Column<short>(type: "smallint", nullable: false),
                    FirmwareVersion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VersionReader0 = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    VersionReader1 = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    NfcModuleVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExtraVersion = table.Column<string>(type: "text", nullable: true),
                    RegisterIdNumber = table.Column<int>(type: "integer", nullable: false),
                    EventCount = table.Column<int>(type: "integer", nullable: false),
                    NumberOfNotTransmittingEvent = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeviceBuzzer = table.Column<short>(type: "smallint", nullable: false),
                    MealServiceTimeId = table.Column<int>(type: "integer", nullable: true),
                    AliasId = table.Column<string>(type: "text", nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true),
                    UseAlarmRelay = table.Column<bool>(type: "boolean", nullable: false),
                    ElevatorId = table.Column<int>(type: "integer", nullable: true),
                    TwoPartSystemId = table.Column<int>(type: "integer", nullable: true),
                    ControllerId = table.Column<int>(type: "integer", nullable: true),
                    DeviceManagerIds = table.Column<string>(type: "text", nullable: true),
                    DependentDoors = table.Column<string>(type: "text", nullable: true),
                    AccountId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IcuDevice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IcuDevice_AccessTime",
                        column: x => x.PassageTzId,
                        principalTable: "AccessTime",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IcuDevice_AccessTime1",
                        column: x => x.ActiveTzId,
                        principalTable: "AccessTime",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IcuDevice_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IcuDevice_Building",
                        column: x => x.BuildingId,
                        principalTable: "Building",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IcuDevice_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IcuDevice_ControllerDevice",
                        column: x => x.ControllerId,
                        principalTable: "ControllerDevice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IcuDevice_Elevator_ElevatorId",
                        column: x => x.ElevatorId,
                        principalTable: "Elevator",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IcuDevice_MealServiceTime_MealServiceTimeId",
                        column: x => x.MealServiceTimeId,
                        principalTable: "MealServiceTime",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IcuDevice_TwoPartSystem",
                        column: x => x.TwoPartSystemId,
                        principalTable: "TwoPartSystem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SystemLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    Content = table.Column<string>(type: "character varying", nullable: true),
                    ContentDetails = table.Column<string>(type: "character varying", nullable: true),
                    ContentIds = table.Column<string>(type: "character varying", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    OpeTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemLog_Account",
                        column: x => x.CreatedBy,
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SystemLog_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DepartmentAccessGroup",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "integer", nullable: false),
                    AccessGroupId = table.Column<int>(type: "integer", nullable: false)
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
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    UserCode = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    AccountId = table.Column<int>(type: "integer", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DepartmentId = table.Column<int>(type: "integer", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ExpiredDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EmpNumber = table.Column<string>(type: "text", nullable: true),
                    FirstName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    HomePhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Job = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    KeyPadPw = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Nationality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OfficePhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Position = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PostCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Remarks = table.Column<string>(type: "character varying", nullable: true),
                    Responsibility = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Rfu = table.Column<string>(type: "text", nullable: true),
                    Sex = table.Column<bool>(type: "boolean", nullable: false),
                    AccessGroupId = table.Column<int>(type: "integer", nullable: false),
                    IsMasterCard = table.Column<bool>(type: "boolean", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    PassType = table.Column<short>(type: "smallint", nullable: false),
                    WorkType = table.Column<short>(type: "smallint", nullable: true),
                    PermissionType = table.Column<short>(type: "smallint", nullable: false),
                    IsSystemUseApply = table.Column<bool>(type: "boolean", nullable: false),
                    SystemUseApplyReason = table.Column<string>(type: "text", nullable: true),
                    SystemUsePassword = table.Column<string>(type: "text", nullable: true),
                    IsSystemUseApproval = table.Column<bool>(type: "boolean", nullable: false),
                    SystemAuth = table.Column<string>(type: "text", nullable: true),
                    IsAccountLock = table.Column<bool>(type: "boolean", nullable: false),
                    SystemUseApplyDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Grade = table.Column<string>(type: "text", nullable: true),
                    NationalIdNumber = table.Column<string>(type: "text", nullable: true),
                    ApproverId1 = table.Column<int>(type: "integer", nullable: false),
                    ApproverId2 = table.Column<int>(type: "integer", nullable: false),
                    ApprovalStatus = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    BirthDay = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    WorkingTypeId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_AccessGroup",
                        column: x => x.AccessGroupId,
                        principalTable: "AccessGroup",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_User_Account",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_User_Company1",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_User_Department",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_User_WorkingType",
                        column: x => x.WorkingTypeId,
                        principalTable: "WorkingType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AccessGroupDevice",
                columns: table => new
                {
                    AccessGroupId = table.Column<int>(type: "integer", nullable: false),
                    IcuId = table.Column<int>(type: "integer", nullable: false),
                    TzId = table.Column<int>(type: "integer", nullable: false),
                    FloorIds = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessGroupDevice", x => new { x.AccessGroupId, x.IcuId });
                    table.ForeignKey(
                        name: "FK_AccessGroupDevice_AccessTime",
                        column: x => x.TzId,
                        principalTable: "AccessTime",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AccessGroupDevice_IcuDevice",
                        column: x => x.IcuId,
                        principalTable: "IcuDevice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessGroupDevice_User",
                        column: x => x.AccessGroupId,
                        principalTable: "AccessGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Camera",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    PlaceID = table.Column<int>(type: "integer", nullable: false),
                    IcuDeviceId = table.Column<int>(type: "integer", nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    VideoLength = table.Column<int>(type: "integer", nullable: false),
                    ConnectionStatus = table.Column<short>(type: "smallint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    SaveEventUnknownFace = table.Column<bool>(type: "boolean", nullable: false),
                    SaveEventCommunication = table.Column<bool>(type: "boolean", nullable: false),
                    CheckEventFromWebHook = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CameraId = table.Column<string>(type: "text", nullable: true),
                    RoleReader = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Camera", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Camera_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Camera_IcuDevice_IcuDeviceId",
                        column: x => x.IcuDeviceId,
                        principalTable: "IcuDevice",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DepartmentDevice",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "integer", nullable: false),
                    IcuId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentDevice", x => new { x.DepartmentId, x.IcuId });
                    table.ForeignKey(
                        name: "FK_DepartmentDevice_Department",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepartmentDevice_IcuDevice",
                        column: x => x.IcuId,
                        principalTable: "IcuDevice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExceptionalMeal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Start = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    FromMealTypeId = table.Column<int>(type: "integer", nullable: false),
                    ToMealTypeId = table.Column<int>(type: "integer", nullable: false),
                    icuDeviceId = table.Column<int>(type: "integer", nullable: true)
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
                name: "Attendance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    StartD = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndD = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ClockInD = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ClockOutD = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimeZone = table.Column<string>(type: "text", nullable: true),
                    TotalWorkingTime = table.Column<double>(type: "double precision", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    WorkingTime = table.Column<string>(type: "jsonb", nullable: true),
                    EditedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendance_Company",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attendance_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceLeave",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    EditedBy = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    RejectReason = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceLeave", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceLeave_Company",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttendanceLeave_Company_CompanyId",
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
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    BuildingId = table.Column<int>(type: "integer", nullable: false)
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
                name: "Card",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IssuedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CardType = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    VisitId = table.Column<int>(type: "integer", nullable: true),
                    UnknownPersonId = table.Column<int>(type: "integer", nullable: true),
                    CardName = table.Column<string>(type: "text", nullable: true),
                    CardId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IssueCount = table.Column<int>(type: "integer", nullable: false),
                    CardStatus = table.Column<short>(type: "smallint", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    AccessGroupId = table.Column<int>(type: "integer", nullable: false),
                    IsMasterCard = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    Etc = table.Column<string>(type: "text", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ManagementNumber = table.Column<string>(type: "text", nullable: true),
                    CardRole = table.Column<short>(type: "smallint", nullable: false),
                    CardRoleType = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Card", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Card_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Card_UnknownPerson",
                        column: x => x.UnknownPersonId,
                        principalTable: "UnknownPerson",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Card_User",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Card_Visit",
                        column: x => x.VisitId,
                        principalTable: "Visit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Face",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    LeftIrisImage = table.Column<string>(type: "text", nullable: true),
                    RightIrisImage = table.Column<string>(type: "text", nullable: true),
                    FaceImage = table.Column<string>(type: "text", nullable: true),
                    FaceSmallImage = table.Column<string>(type: "text", nullable: true),
                    LeftIrisCode = table.Column<string>(type: "text", nullable: true),
                    RightIrisCode = table.Column<string>(type: "text", nullable: true),
                    FaceCode = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Face", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Face_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Face_User",
                        column: x => x.UserId,
                        principalTable: "User",
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
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Contact = table.Column<string>(type: "text", nullable: true),
                    HourlyWage = table.Column<int>(type: "integer", nullable: false),
                    AccountNumber = table.Column<string>(type: "text", nullable: true),
                    Bank = table.Column<string>(type: "text", nullable: true),
                    AccountHolder = table.Column<string>(type: "text", nullable: true)
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
                name: "UserCategoryOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CategoryOptionId = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false)
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
                name: "UserDiscount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: false)
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
                name: "Vehicle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Model = table.Column<string>(type: "text", nullable: true),
                    PlateNumber = table.Column<string>(type: "text", nullable: true),
                    Color = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    VisitId = table.Column<int>(type: "integer", nullable: true),
                    DepartmentId = table.Column<int>(type: "integer", nullable: true),
                    VehicleType = table.Column<int>(type: "integer", nullable: false),
                    VehicleClass = table.Column<int>(type: "integer", nullable: false),
                    ExistBlackBox = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Purpose = table.Column<string>(type: "text", nullable: true),
                    PlateRFID = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    VehicleRule = table.Column<int>(type: "integer", nullable: false),
                    VehicleName = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicle_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vehicle_Department",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vehicle_User",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vehicle_Visit",
                        column: x => x.VisitId,
                        principalTable: "Visit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Antipass = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CardId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IssueCount = table.Column<int>(type: "integer", nullable: false),
                    CardType = table.Column<short>(type: "smallint", nullable: false),
                    CardStatus = table.Column<short>(type: "smallint", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DeptId = table.Column<int>(type: "integer", nullable: true),
                    DoorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EventTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    IcuId = table.Column<int>(type: "integer", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false),
                    KeyPadPw = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    ImageCamera = table.Column<string>(type: "jsonb", nullable: true),
                    ResultCheckIn = table.Column<string>(type: "text", nullable: true),
                    Videos = table.Column<string>(type: "text", nullable: true),
                    BodyTemperature = table.Column<double>(type: "double precision", nullable: false),
                    DelayOpenDoorByCamera = table.Column<double>(type: "double precision", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    IsVisit = table.Column<bool>(type: "boolean", nullable: false),
                    VisitId = table.Column<int>(type: "integer", nullable: true),
                    CameraId = table.Column<int>(type: "integer", nullable: true),
                    UnknownPersonId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventLog_Camera",
                        column: x => x.CameraId,
                        principalTable: "Camera",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventLog_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EventLog_IcuDevice",
                        column: x => x.IcuId,
                        principalTable: "IcuDevice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventLog_UnknownPerson",
                        column: x => x.UnknownPersonId,
                        principalTable: "UnknownPerson",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventLog_User",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventLog_Visit",
                        column: x => x.VisitId,
                        principalTable: "Visit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportProblem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Detail = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    IcuDeviceId = table.Column<int>(type: "integer", nullable: true),
                    CameraId = table.Column<int>(type: "integer", nullable: true)
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
                name: "AttendanceLeaveRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AttendanceId = table.Column<int>(type: "integer", nullable: false),
                    AttendanceLeaveId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceLeaveRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceLeaveRequest_Attendance",
                        column: x => x.AttendanceId,
                        principalTable: "Attendance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttendanceLeaveRequest_AttendanceLeave",
                        column: x => x.AttendanceLeaveId,
                        principalTable: "AttendanceLeave",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CardId = table.Column<int>(type: "integer", nullable: false),
                    CardName = table.Column<string>(type: "text", nullable: true),
                    CardNo = table.Column<string>(type: "text", nullable: true),
                    IssueCount = table.Column<int>(type: "integer", nullable: false),
                    CardStatus = table.Column<short>(type: "smallint", nullable: false),
                    CardRoleType = table.Column<short>(type: "smallint", nullable: false),
                    ManagementNumber = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    VisitId = table.Column<int>(type: "integer", nullable: true),
                    AccidentTypeId = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false)
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
                name: "FingerPrint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Templates = table.Column<string>(type: "text", nullable: true),
                    CardId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FingerPrint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FingerPrint_Card",
                        column: x => x.CardId,
                        principalTable: "Card",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleAllocation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ManagerId = table.Column<int>(type: "integer", nullable: false),
                    DriverIds = table.Column<string>(type: "text", nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DispatchType = table.Column<string>(type: "text", nullable: true),
                    SupportType = table.Column<string>(type: "text", nullable: true),
                    Purpose = table.Column<string>(type: "text", nullable: true),
                    Destination = table.Column<string>(type: "text", nullable: true),
                    UnitVehicleId = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    ServiceStartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ServiceEndTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "EventMemo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventLogId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false)
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
                name: "MealEventLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventLogId = table.Column<int>(type: "integer", nullable: false),
                    MealType = table.Column<string>(type: "text", nullable: true),
                    MealCode = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    ExceptionalMealAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    ExceptionalUserAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    AppliedDiscount = table.Column<int>(type: "integer", nullable: false),
                    IsManual = table.Column<bool>(type: "boolean", nullable: false),
                    CornerId = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_AccessGroup_CompanyId",
                table: "AccessGroup",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessGroup_ParentId",
                table: "AccessGroup",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessGroupDevice_AccessGroupId",
                table: "AccessGroupDevice",
                column: "AccessGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessGroupDevice_IcuId",
                table: "AccessGroupDevice",
                column: "IcuId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessGroupDevice_TzId",
                table: "AccessGroupDevice",
                column: "TzId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessSetting_CompanyId",
                table: "AccessSetting",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccessTime_CompanyId",
                table: "AccessTime",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AccidentType_CompanyId",
                table: "AccidentType",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AccidentType_Id",
                table: "AccidentType",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Account_CompanyId",
                table: "Account",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Account_CreateDateRefreshToken",
                table: "Account",
                column: "CreateDateRefreshToken");

            migrationBuilder.CreateIndex(
                name: "IX_Account_DynamicRoleId",
                table: "Account",
                column: "DynamicRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalHistory_CompanyId",
                table: "ApprovalHistory",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_CompanyId",
                table: "Attendance",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_UserId",
                table: "Attendance",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceLeave_CompanyId",
                table: "AttendanceLeave",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceLeave_UserId",
                table: "AttendanceLeave",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceLeaveRequest_AttendanceId",
                table: "AttendanceLeaveRequest",
                column: "AttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceLeaveRequest_AttendanceLeaveId",
                table: "AttendanceLeaveRequest",
                column: "AttendanceLeaveId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceLeaveRequest_Id",
                table: "AttendanceLeaveRequest",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceSetting_CompanyId",
                table: "AttendanceSetting",
                column: "CompanyId",
                unique: true);

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
                name: "IX_Building_CompanyId",
                table: "Building",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Building_ParentId",
                table: "Building",
                column: "ParentId");

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
                name: "IX_Camera_CameraId",
                table: "Camera",
                column: "CameraId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Camera_CompanyId",
                table: "Camera",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Camera_IcuDeviceId",
                table: "Camera",
                column: "IcuDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Camera_Id",
                table: "Camera",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Camera_Name",
                table: "Camera",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Card_CompanyId",
                table: "Card",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Card_UnknownPersonId",
                table: "Card",
                column: "UnknownPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Card_UserId",
                table: "Card",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Card_VisitId",
                table: "Card",
                column: "VisitId");

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
                name: "IX_CompanyAccount_AccountId",
                table: "CompanyAccount",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyAccount_CompanyId",
                table: "CompanyAccount",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyAccount_CompanyId_AccountId",
                table: "CompanyAccount",
                columns: new[] { "CompanyId", "AccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyAccount_DynamicRoleId",
                table: "CompanyAccount",
                column: "DynamicRoleId");

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
                name: "IX_DataListSetting_AccountId",
                table: "DataListSetting",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_DataListSetting_CompanyId",
                table: "DataListSetting",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DebugMessage_Id",
                table: "DebugMessage",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Department_CompanyId",
                table: "Department",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Department_DepartmentManagerId",
                table: "Department",
                column: "DepartmentManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Department_ParentId",
                table: "Department",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAccessGroup_AccessGroupId",
                table: "DepartmentAccessGroup",
                column: "AccessGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAccessGroup_DepartmentId",
                table: "DepartmentAccessGroup",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentDevice_DepartmentId",
                table: "DepartmentDevice",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentDevice_IcuId",
                table: "DepartmentDevice",
                column: "IcuId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMessage_CompanyId",
                table: "DeviceMessage",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicRole_CompanyId",
                table: "DynamicRole",
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
                name: "IX_EventLog_CameraId",
                table: "EventLog",
                column: "CameraId");

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_CompanyId",
                table: "EventLog",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_CompanyId_IcuId_EventTime_CardId",
                table: "EventLog",
                columns: new[] { "CompanyId", "IcuId", "EventTime", "CardId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_IcuId",
                table: "EventLog",
                column: "IcuId");

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_Id",
                table: "EventLog",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_UnknownPersonId",
                table: "EventLog",
                column: "UnknownPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_UserId",
                table: "EventLog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_VisitId",
                table: "EventLog",
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
                name: "IX_Face_CompanyId",
                table: "Face",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Face_Id",
                table: "Face",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Face_UserId",
                table: "Face",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FingerPrint_CardId",
                table: "FingerPrint",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_FingerPrint_Id",
                table: "FingerPrint",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_FirmwareVersion_Id",
                table: "FirmwareVersion",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_HeaderSetting_AccountId",
                table: "HeaderSetting",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_HeaderSetting_CompanyId",
                table: "HeaderSetting",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Holiday_CompanyId",
                table: "Holiday",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_IcuDevice_AccountId",
                table: "IcuDevice",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_IcuDevice_ActiveTzId",
                table: "IcuDevice",
                column: "ActiveTzId");

            migrationBuilder.CreateIndex(
                name: "IX_IcuDevice_BuildingId",
                table: "IcuDevice",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_IcuDevice_CompanyId",
                table: "IcuDevice",
                column: "CompanyId");

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
                name: "IX_IcuDevice_PassageTzId",
                table: "IcuDevice",
                column: "PassageTzId");

            migrationBuilder.CreateIndex(
                name: "IX_IcuDevice_TwoPartSystemId",
                table: "IcuDevice",
                column: "TwoPartSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_IssuingDevice_CompanyId",
                table: "IssuingDevice",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_IssuingDevice_Id",
                table: "IssuingDevice",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequestSetting_CompanyId",
                table: "LeaveRequestSetting",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequestSetting_Id",
                table: "LeaveRequestSetting",
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
                name: "IX_Meeting_Name",
                table: "Meeting",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageLog_CompanyId",
                table: "MessageLog",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageLog_Id",
                table: "MessageLog",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_CompanyId",
                table: "Notification",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_Id",
                table: "Notification",
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
                name: "IX_PlugIn_CompanyId",
                table: "PlugIn",
                column: "CompanyId");

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
                name: "IX_ShortenLink_FullPath",
                table: "ShortenLink",
                column: "FullPath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortenLink_Id",
                table: "ShortenLink",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ShortenLink_ShortPath",
                table: "ShortenLink",
                column: "ShortPath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SymptomCovid_Id",
                table: "SymptomCovid",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SymptomCovid_VisitId",
                table: "SymptomCovid",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemInfo_Id",
                table: "SystemInfo",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLog_CompanyId",
                table: "SystemLog",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLog_CreatedBy",
                table: "SystemLog",
                column: "CreatedBy");

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
                name: "IX_UnregistedDevice_CompanyId",
                table: "UnregistedDevice",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_UnregistedDevice_Id",
                table: "UnregistedDevice",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_User_AccessGroupId",
                table: "User",
                column: "AccessGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_User_AccountId",
                table: "User",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_User_CompanyId",
                table: "User",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_User_DepartmentId",
                table: "User",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_User_WorkingTypeId",
                table: "User",
                column: "WorkingTypeId");

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
                name: "IX_Vehicle_CompanyId",
                table: "Vehicle",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_DepartmentId",
                table: "Vehicle",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_Id",
                table: "Vehicle",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_UserId",
                table: "Vehicle",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_VisitId",
                table: "Vehicle",
                column: "VisitId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Visit_AccessGroupId",
                table: "Visit",
                column: "AccessGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Visit_CompanyId",
                table: "Visit",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitHistory_CompanyId",
                table: "VisitHistory",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitSetting_AccessTimeId",
                table: "VisitSetting",
                column: "AccessTimeId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitSetting_CompanyId",
                table: "VisitSetting",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkingType_CompanyId",
                table: "WorkingType",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessGroupDevice");

            migrationBuilder.DropTable(
                name: "AccessSetting");

            migrationBuilder.DropTable(
                name: "ApprovalHistory");

            migrationBuilder.DropTable(
                name: "AttendanceLeaveRequest");

            migrationBuilder.DropTable(
                name: "AttendanceSetting");

            migrationBuilder.DropTable(
                name: "BookBorrowTicket");

            migrationBuilder.DropTable(
                name: "BuildingMaster");

            migrationBuilder.DropTable(
                name: "CardHistory");

            migrationBuilder.DropTable(
                name: "CardLayout");

            migrationBuilder.DropTable(
                name: "CompanyAccount");

            migrationBuilder.DropTable(
                name: "DataListSetting");

            migrationBuilder.DropTable(
                name: "DebugMessage");

            migrationBuilder.DropTable(
                name: "DepartmentAccessGroup");

            migrationBuilder.DropTable(
                name: "DepartmentDevice");

            migrationBuilder.DropTable(
                name: "DeviceMessage");

            migrationBuilder.DropTable(
                name: "ElevatorFloor");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "Event");

            migrationBuilder.DropTable(
                name: "EventMemo");

            migrationBuilder.DropTable(
                name: "ExceptionalMeal");

            migrationBuilder.DropTable(
                name: "Face");

            migrationBuilder.DropTable(
                name: "FingerPrint");

            migrationBuilder.DropTable(
                name: "FirmwareVersion");

            migrationBuilder.DropTable(
                name: "HeaderSetting");

            migrationBuilder.DropTable(
                name: "Holiday");

            migrationBuilder.DropTable(
                name: "IssuingDevice");

            migrationBuilder.DropTable(
                name: "LeaveRequestSetting");

            migrationBuilder.DropTable(
                name: "MailTemplate");

            migrationBuilder.DropTable(
                name: "MealEventLog");

            migrationBuilder.DropTable(
                name: "MealSetting");

            migrationBuilder.DropTable(
                name: "Meeting");

            migrationBuilder.DropTable(
                name: "MessageLog");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "PartTime");

            migrationBuilder.DropTable(
                name: "PersonCardType");

            migrationBuilder.DropTable(
                name: "PlugIn");

            migrationBuilder.DropTable(
                name: "ReportProblem");

            migrationBuilder.DropTable(
                name: "Setting");

            migrationBuilder.DropTable(
                name: "ShortenLink");

            migrationBuilder.DropTable(
                name: "SymptomCovid");

            migrationBuilder.DropTable(
                name: "SystemInfo");

            migrationBuilder.DropTable(
                name: "SystemLog");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "UnregistedDevice");

            migrationBuilder.DropTable(
                name: "UserCategoryOption");

            migrationBuilder.DropTable(
                name: "UserDiscount");

            migrationBuilder.DropTable(
                name: "VehicleAllocation");

            migrationBuilder.DropTable(
                name: "VisitHistory");

            migrationBuilder.DropTable(
                name: "VisitSetting");

            migrationBuilder.DropTable(
                name: "Attendance");

            migrationBuilder.DropTable(
                name: "AttendanceLeave");

            migrationBuilder.DropTable(
                name: "Book");

            migrationBuilder.DropTable(
                name: "AccidentType");

            migrationBuilder.DropTable(
                name: "Card");

            migrationBuilder.DropTable(
                name: "EventLog");

            migrationBuilder.DropTable(
                name: "CornerSetting");

            migrationBuilder.DropTable(
                name: "MealType");

            migrationBuilder.DropTable(
                name: "Credit");

            migrationBuilder.DropTable(
                name: "CategoryOption");

            migrationBuilder.DropTable(
                name: "Vehicle");

            migrationBuilder.DropTable(
                name: "BookArea");

            migrationBuilder.DropTable(
                name: "Camera");

            migrationBuilder.DropTable(
                name: "UnknownPerson");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Visit");

            migrationBuilder.DropTable(
                name: "IcuDevice");

            migrationBuilder.DropTable(
                name: "Department");

            migrationBuilder.DropTable(
                name: "WorkingType");

            migrationBuilder.DropTable(
                name: "AccessGroup");

            migrationBuilder.DropTable(
                name: "AccessTime");

            migrationBuilder.DropTable(
                name: "ControllerDevice");

            migrationBuilder.DropTable(
                name: "Elevator");

            migrationBuilder.DropTable(
                name: "MealServiceTime");

            migrationBuilder.DropTable(
                name: "TwoPartSystem");

            migrationBuilder.DropTable(
                name: "Account");

            migrationBuilder.DropTable(
                name: "Building");

            migrationBuilder.DropTable(
                name: "DynamicRole");

            migrationBuilder.DropTable(
                name: "Company");
        }
    }
}
