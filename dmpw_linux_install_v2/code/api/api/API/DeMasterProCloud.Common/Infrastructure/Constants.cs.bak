// ReSharper disable All
using DeMasterProCloud.Common.Infrastructure.Header;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;

namespace DeMasterProCloud.Common.Infrastructure
{
    public class ColumnDefines
    {
        public static readonly string[] MprUser =
        {
            "User.Id",
            "User.UserCode",
            "User.FirstName",
            "User.CardId",
            "User.Department.DepartName"
        };

        public static readonly string[] Account = {
            //"Id",
            "Username",
            "Company.Name",
            "Type",
            //"Status"
        };

        public static readonly string[] CompanyAccount = {
            "Account.Username",
            "Company.Name",
            "DynamicRole.TypeId",
        };

        public static readonly string[] AccountListModel = {
            "Email",
            "UserName",
            "Role"
        };

        //public static readonly string[] Department = {
        //    "Id",
        //    "DepartNo",
        //    "DepartName",
        //    "ParentId"
        //};
        public static readonly string[] Department = {

            "DepartNo",
            "DepartName"

        };

        public static readonly string[] AccessGroup =
        {
            "Id",
            "Name",
            "IsDefault"
        };

        public static readonly string[] AccessGroupForDoors =
        {
            "DeviceAddress",
            "DoorName",
            "Timezone",
            "Building"
        };

        public static readonly string[] AccessGroupForUnAssignDoors =
        {
            "Id",
            "DeviceAddress",
            "DoorName",
            "Building"
        };
        public static readonly string[] DepartmentDeviceForUnAssignDoors =
        {
            "DeviceAddress",
            "DoorName"
        };

        public static readonly string[] UnregistedDevices =
        {
            "Id",
            "DeviceAddress",
            "Status",
            "IpAddress"
        };

        public static readonly string[] AccessGroupForUsers =
        {
            //"Id",
            //"CardId",
            "FullName",
            "DepartmentName"
        };

        public static readonly string[] AccessGroupForUnAssignUsers =
        {
            //"Id",
            //"CardId",
            "FullName",
            "DepartmentName",
            "AccessGroupName"
        };

        public static readonly string[] User = {
            "Id",
            "FirstName",
            "LastName",
            "CardId",
            "Department.DepartName",
            "EmpNumber",
            "ExpiredDate"
        };

        public static readonly string[] RegisteredUser = {
            "0",
            "1",
            "FirstName",
            "Department.DepartName",
            "EmpNumber",
            "ExpiredDate",
            "Card.CardStatus"
        };

        public static readonly string[] NewUser = {
            "FirstName",
            "UserCode",
            "Department.DepartName",
            "EmpNumber",
            "LastName",
            "ExpiredDate"
        };

        public static readonly string[] UserList = {
            //"FirstName",
            //"UserCode",
            //"Department.DepartName",
            //"EmpNumber",
            //"ExpiredDate"
            "0(CardId) - not used",
            "FirstName",
            "Department.DepartName",
            "ExpiredDate",
            "UserCode",
            "EmpNumber",
            "6(IssueCount) - not used",
            "7(CardStatus) - not used",
            "AccessGroup.Name",
            "9(CardList) - not used"
        };

        public static readonly string[] UserArmyList = {
            "0(CardId) - not used",
            "FirstName",
            "DepartmentName",
            "ExpiredDate",
            "UserCode",
            "EmployeeNo",
            "6(IssueCount) - not used",
            "7(CardStatus) - not used",
            "AccessGroupName",
            "9(CardList) - not used"
        };

        public static readonly string[] UserHeader =
        {
            UserHeaderColumns.Id.GetName(),
            UserHeaderColumns.FirstName.GetName(),
            UserHeaderColumns.UserCode.GetName(),
            UserHeaderColumns.DepartmentName.GetName(),
            UserHeaderColumns.Position.GetName(),
            UserHeaderColumns.EmployeeNo.GetName(),
            UserHeaderColumns.ExpiredDate.GetName(),
            UserHeaderColumns.WorkTypeName.GetName(),
            UserHeaderColumns.CardList.GetName(),
            UserHeaderColumns.PlateNumberList.GetName(),
            UserHeaderColumns.Action.GetName(),
            UserHeaderColumns.ApprovalStatus.GetName(),
        };

        public static readonly string[] UserArmyHeader =
        {
            UserHeaderColumns.Id.GetName(),
            UserHeaderColumns.FirstName.GetName(),
            UserHeaderColumns.DepartmentName.GetName(),
            UserHeaderColumns.MilitaryNo.GetName(),
            UserHeaderColumns.Position.GetName(),
            UserHeaderColumns.ExpiredDate.GetName(),
            UserHeaderColumns.WorkTypeName.GetName(),
            UserHeaderColumns.CardList.GetName(),
            UserHeaderColumns.PlateNumberList.GetName(),
            UserHeaderColumns.Action.GetName(),
        };

        /// <summary>
        /// This array has headers that be shown in Accessible User page.
        /// </summary>
        public static readonly string[] AccessibleUserHeader =
        {
            UserHeaderColumns.Id.GetName(),
            UserHeaderColumns.FirstName.GetName(),
            UserHeaderColumns.DepartmentName.GetName(),
            UserHeaderColumns.Position.GetName(),
            UserHeaderColumns.EmployeeNo.GetName(),
            UserHeaderColumns.MilitaryNo.GetName(),
            UserHeaderColumns.WorkTypeName.GetName(),
            UserHeaderColumns.AccessGroupName.GetName(),
            UserHeaderColumns.CardList.GetName(),
        };

        /// <summary>
        /// This array has headers that be shown in ASSIGN UI about unaccessible user in Accessible User page.
        /// </summary>
        public static readonly string[] UnAccessibleUserHeader =
        {
            UserHeaderColumns.Id.GetName(),
            UserHeaderColumns.FirstName.GetName(),
            UserHeaderColumns.DepartmentName.GetName(),
            UserHeaderColumns.Position.GetName(),
            UserHeaderColumns.EmployeeNo.GetName(),
            UserHeaderColumns.MilitaryNo.GetName(),
            UserHeaderColumns.WorkTypeName.GetName(),
            UserHeaderColumns.CardList.GetName(),
        };

        /// <summary>
        /// This array has headers about not assigned users in access group.
        /// </summary>
        public static readonly string[] UnAssignedUserHeader =
        {
            UserHeaderColumns.Id.GetName(),
            UserHeaderColumns.FirstName.GetName(),
            UserHeaderColumns.DepartmentName.GetName(),
            UserHeaderColumns.Position.GetName(),
            UserHeaderColumns.EmployeeNo.GetName(),
            UserHeaderColumns.MilitaryNo.GetName(),
            UserHeaderColumns.CardList.GetName(),
            UserHeaderColumns.PlateNumberList.GetName(),
            UserHeaderColumns.AccessGroupName.GetName()
        };

        /// <summary>
        /// This array has headers about accounts information.
        /// </summary>
        public static readonly string[] AccountHeader =
        {
            UserHeaderColumns.Id.GetName(),
            UserHeaderColumns.Email.GetName(),
            UserHeaderColumns.FirstName.GetName(),
            UserHeaderColumns.UserCode.GetName(),
            UserHeaderColumns.DepartmentName.GetName(),
            UserHeaderColumns.Position.GetName(),
        };


        public static readonly string[] VehicleEventLogHeader =
        {
            VehicleEventLogHeaderColumn.Id.GetName(),
            VehicleEventLogHeaderColumn.EventTime.GetName(),
            VehicleEventLogHeaderColumn.DoorName.GetName(),
            VehicleEventLogHeaderColumn.Model.GetName(),
            VehicleEventLogHeaderColumn.PlateNumber.GetName(),
            VehicleEventLogHeaderColumn.DepartmentName.GetName(),
            VehicleEventLogHeaderColumn.UserName.GetName(),
            VehicleEventLogHeaderColumn.EventDetail.GetName(),
            VehicleEventLogHeaderColumn.InOut.GetName(),
            VehicleEventLogHeaderColumn.VehicleImage.GetName(),
            VehicleEventLogHeaderColumn.VehicleType.GetName()
        };

        public static readonly string[] UserMasterCard = {
            "CardId",
            "UserName"
        };

        public static readonly string[] Device = {
            "Id",
            "Name",
            "DeviceAddress",
            "ActiveTz.Name",
            "PassageTz.Name",
            "VerifyMode",
            "Status"
        };

        public static readonly string[] DeviceSetting = {
            "Name",
            "DeviceAddress",
            "ActiveTz.Name",
            "PassageTz.Name",
            "VerifyMode",
            "Building.Name",
            "ConnectionStatus"
        };

        public static readonly string[] Devices = {
            "Building.Name",
            "Name",
            "DeviceAddress",
            "ActiveTz.Name",
            "PassageTz.Name",
            "VerifyMode",
            "Company.Name",
            "ConnectionStatus",
            "RegisterIdNumber",
            "DeviceType"
        };

        public static readonly string[] PaginatedDevices = {
            "CompanyName",
            "Building",
            "DoorName",
            "DeviceAddress",
            "DoorActiveTimeZone",
            "DoorPassageTimeZone",
            "VerifyMode",
            "CompanyName",
            "ConnectionStatus",
            "RegisterIdNumber",
            "DeviceType"
        };

        public static readonly string[] AccessibleDoor = {
            "Id",
            "Name",
            "DeviceAddress",
            "ActiveTz.Name",
            "PassageTz.Name",
            "VerifyMode",
            "PassbackRule",
            "DeviceType",
            "MPRCount"
        };

        public static readonly string[] DeviceValid = {
            //"Name",
            //"DeviceAddress",
            //"FirmwareVersion",
            //"LastCommunicationTime",
            //"DeviceType",
            //"NumberOfNotTransmittingEvent",
            //"RegisterIdNumber",
            //"ConnectionStatus",
            //"DoorStatus"
            "Company.Name",
            "Building.Name",
            "Name",
            "DeviceAddress",
            "FirmwareVersion",
            "LastCommunicationTime",
            "DeviceType",
            "NumberOfNotTransmittingEvent",
            "RegisterIdNumber",
            "ConnectionStatus",
            "DoorStatus",
            "UpTimeOnlineDevice"
        };

        public static readonly string[] AttendanceLeave =
        {
            "Status",
            "Id",
            "Date",
            "UserName",
            "Type",
            "Start",
            "End",
            "Reason"
        };


        public static readonly string[] DeviceListModelForUser = {
            "Name",
            "DeviceAddress"
        };

        public static readonly string[] AccessLevelAssignedUsers = {
            "User.UserCode",
            "User.FirstName",
            "User.Department.DepartName",
            "Tz.Name",
            "User.CardId"
        };

        public static readonly string[] AccessLevelUnAssignedUsers = {
            "UserCode",
            "FirstName",
            "Department.DepartName"
        };

        public static readonly string[] Company = {
            "Id",
            "Code",
            "Name",
            "Admin",
            "ExpiredDate",
            "Status",
            "CreatedDate"
        };

        public static readonly string[] CornerSetting = {
            "Id",
            "CompanyId",
            "Code",
            "Name",
            "Description"
        };

        public static readonly string[] MealSetting = {
            "Id",
            "IcuDeviceId",
            "CornerId",
            "MealTypeId",
            "Start",
            "End",
            "Price"
        };

        public static readonly string[] MealEventLog =
        {
             "eventTime",
             "icuId",
             "userName",
             "userId",
             "mealCode",
             "mealType",
             "Price",
             "doorName",
             "mealEventLogId",
             "eventLogId",
             "deviceName",
             "companyId",
             "isManual",
             "deptId",
             "tring departmentName",
             "userCode",
             "cardId",
             "totalPrice",
             "employeeNumber",
             "birthayDay",
             "buildingId",
             "deviceAddress",
             "buildingName"
        };

        public static readonly string[] ListMealSetting = {
            "cornerName",
            "mealTypeName",
            "price",
        };

        public static readonly string[] ExceptionalMeal = {
            "Id",
            "MealSettingId",
            "Start",
            "End",
            "Price"
        };

        public static readonly string[] UserDiscount = {
            "Id",
            "UserId",
            "Amount",
        };

        public static readonly string[] AccessTime = {
            "AccessTimeName"
        };

        public static readonly string[] Holiday = {
            "Name",
            "HolidayType",
            "StartDate",
            "EndDate",
            "RecursiveDisp",
        };

        public static readonly string[] EventLog =
        {
            "EventTime",
            "User.UserCode",
            "User.FirstName",
            "CardId",
            "KeyPadPw",
            "Icu.DeviceAddress",
            "DoorName",
            "CardType",
            "Antipass",
            "EventType"
        };

        public static readonly string[] Notification =
        {
            "Type",
            "CreatedOn"
        };

        public static readonly string[] EventLogForReport =
        {
            //"Id",
            "EventTime",
            "UserName",
            "User.BirthDay",
            "User.UserCode",
            "User.Department.DepartName",
            "CardId",
            "Icu.DeviceAddress",
            "DoorName",
            "Icu.Building.Name",
            "Antipass",
            "EventType",
            "IssueCount",
            "CardStatus",
            "CardType"
        };

        public static readonly string[] SystemLog =
        {
            "CreatedByNavigation.Username",
            "OpeTime",
            "Type",
            "Action",
            "Content"
        };

        public static readonly string[] SystemLogForReport =
        {
            "OperationTime",
            "UserAccount",
            "OperationType",
            "Action",
            "Details"
        };

        public static readonly string[] SystemLogListModelForReport =
        {
            "OperationTime",
            "UserAccount",
            "OperationType",
            "Action",
            "Message"
        };

        public static readonly string[] MessageLog =
        {
            "Id",
            "MsgId",
            "Topic",
            "PayLoad",
            "Status",
            "PublishedTime",
            "ResponseTime",
            "Type",
            "GroupMsgId"
        };

        public static readonly string[] WgSetting = {
            "Id",
            "Name",
            "TotalBits",
            "TypeDisp",
            "CreatedOnDisp"
        };
        public static readonly string[] BuildingList = {
            "Id",
            "Name",
            "Edit",
            "Delete"
        };
        public static readonly string[] BuildingUnAssignDoorList = {
            "Id",
            "Building",
            "DoorName",
            "DeviceAddress"
        };
        public static readonly string[] BuildingDoorList = {
            "DoorName",
            "DeviceAddress",
            "DeviceType",
            "ActiveTz",
            "PassageTz",
        };

        public static readonly string[] DeviceMessageList = {

            "MessageId",
            "Content"

        };

        public static readonly string[] Visit = {

            "ApplyDate",
            "VisitorName",
            "BirthDay",
            "VisitorDepartment",
            "Position",
            "StartDate",
            "EndDate",
            "VisiteeSite",
            "VisiteeReason",
            "VisiteeName",
            "Phone",
            "Status",
            "ApproverId1",
            "ApproverId2",
            "RejectReason",
            "CardId"

        };

        public static readonly string[] VisitReport =
        {
            "Id",
            "EventTime",
            "Visit.VisitorName",
            "Visit.BirthDay",
            "Visit.VisitorDepartment",
            "CardId",
            "Icu.DeviceAddress",
            "Icu.Name",
            "Building.BuildingName",
            "Icu.HardAntiFlag",
            "EventDetails",
            "IssueCount",
            "CardStatus",
        };


        // canteen

        public static readonly string[] MealType =
        {
            "Id",
            "Name",
            "Code",
            "Description"
        };
        
        // header device
        public static readonly string[] HeaderDeviceOs =
        {
            "Device-Model",
            "Device-OS-Name",
            "Device-OS-Version",
            "Device-UUID"
            
        };
    }

    /// <summary>
    /// Define all Constants
    /// </summary>
    public class Constants
    {
        public static int MaxSendIcuUserCount = 10;
        public static int MaxSendITouchPopUserCount = 1000;
        public static int MaxSendPopXCount = 50;
        public static int MaxSendAndroidCount = 100;
        public static int MaxSendUserDelay = 0;
        public static int MaxSendUserEbknToDMP = 10;
        public const int DefaultPaginationNumber = 10;
        public const string Description = "Description";
        // public const int MaxDeviceAddressDigit = 6;
        public const int MaxControllerAddressDigit = 10;
        public const string MaxSplitUserInICU = "MaxSplitUserInICU";
        public const string CommaString = ",";
        public const char Colon = ':';
        public const string AllowImageType = "AllowImageType";
        public const string DateServerFormat = "DateServerFormat";
        public const string DateTimeServerFormat = "DateTimeServerFormat";
        public const string DateTimeWithoutSecServerFormat = "DateTimeWithoutSecServerFormat";
        public const string TimeToSendSetTimeEveryDay = "TimeToSendSetTimeEveryDay";
        public const string TimeToSendSetTimeHours = "TimeToSendSetTimeHours";
        public const string TimeToSendSetTimeMinutes = "TimeToSendSetTimeMinutes";
        public const string AccessTimeFormat = "AccessTimeFormat";
        public const string ImportExportUserFormat = "ImportExportUserFormat";
        public const string UpdateTimeFormat = "UpdateTimeFormat";
        public const string DateClientFormat = "DateClientFormat";
        public const string AppName = "DemasterPro";
        public const string CacheBuster = "CacheBuster";
        public const string Alphabet = "abcdefghijklmnopqrstuvwxyz";
        public const string AlphabetAndNumber = "ABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
        public const string HexNumber = "ABCDEF0123456789";
        public const string Ok = "OK";
        public const string Ng = "NG";
        public const string Excel = "excel";
        public const string Hancell = "hancell";
        public const string CSV = "csv";
        public const string Txt = "txt";
        public const string WorkSheetName = "User";
        public const string UploadDir = "Upload";
        public const string TutorialsDir = "Tutorials";
        public const string ExportFileFormat = "{0}_{1:yyyyMMddHHmmss}";
        //public const string ExportFileFormat2 = "{0}_{1:yyyyMMddHHmmss}";
        public const string AppSession = ".DesmasterProWeb.Session";
        public const string DefaultConnectionString = "ConnectionStrings:DefaultConnection";
        public const string WeirdCharacter = "ÿ";
        public static readonly string[] RandomChars = {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
                "abcdefghijkmnopqrstuvwxyz",    // lowercase
                "0123456789",                   // digits
                "!@$?_-"                        // non-alphanumeric
            };
        public const string Gps = "<a href=\"{0}\" target=\"_blank\">{1}</a>";
        public const string SpInfo = "<a href=\"{0}\" data-target=\"#spdetailreport\" class=\"btn-detail\" >{1}</a>";
        public static readonly string[] AntiPass = { "In", "Out", "Self", "Power", "Normal", "Timezone" };
        public static int DefaultCompanyId = 1;
        public static int TimeAttendanceEventLogLimit = 500;
        public const int TzNotUsePos = 0;
        public const int Tz24hPos = 1;

        public const string FormatBuilding = "{building}";
        public const string FormatDoor = "{door}";
        public const string categoryVariable = "category";
        public const string DefaultDeviceAddress = "FFFFFF";
        public const string AlphabetFull = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        public const int CountRandomLinkShorten = 6;
        public const string DateTimeFormatDefault = "dd.MM.yyyy HH:mm:ss";
        public const int DefaultPaggingQuery = 100;
        public const string FirmwareVersion2 = "v2"; // popX version 2
        public const int MaxReceiver = 50;
        public const string DefaultTimeZone = "Asia/Ho_Chi_Minh";
        public const string Culture = "culture";
        public const string DefaultAccessGroupName = "AccessGroup-";
        public const long MaximumRequestBodySize = 500 * 1024 * 1024;
        public const string DefaultLanguage = "en-US";

        public class AccessGroup
        {
            //TODO: Full access Group의 경우 언어에 상관없이 FAG라는 이름으로 사용 예정(Import 시에도 FAG로)
            public const string FullAccess_EN = "Full Access";
            public const string FullAccess_KR = "전체 출입";
            public const string FullAccess_JP = "全権アクセス";
            public const string NoAccess_EN = "No Access";
            public const string NoAccess_KR = "출입 불가";
            public const string NoAccess_JP = "アクセスなし";
            public const string VisitAccess_EN = "Visit Access";
            public const string VisitAccess_KR = "방문 출입";
            public const string VisitAccess_JP = "Visit Access";
        }

        public class CardStatus
        {

            public const string Normal_EN = "Normal";
            public const string Normal_KR = "일반";
            public const string Normal_JP = "普通";

            public const string Temp_EN = "Temp";
            public const string Temp_KR = "임시 카드";
            public const string Temp_JP = "温度";

            public const string Retire_EN = "Retire";
            public const string Retire_KR = "퇴직";
            public const string Retire_JP = "引退";

            public const string Lost_EN = "Lost";
            public const string Lost_KR = "분실 카드";
            public const string Lost_JP = "なくした";

            public const string Invalid_EN = "Invalid";
            public const string Invalid_KR = "사용안함";
            public const string Invalid_JP = "無効";
        }

        public class Sex
        {
            public const string Male_EN = "Male";
            public const string Male_en = "male";
            public const string Male_KR = "남성";
            public const string Male_JP = "男性";

            public const string Female_EN = "Female";
            public const string Female_en = "female";
            public const string Female_KR = "여성";
            public const string Female_JP = "女性";
        }

        public class Image
        {
            public const string DefaultMale = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAMgAAAB+CAMAAABWFa7EAAAAMFBMVEXEzeD////Y3uri5vDH0OLl6fL7/P3q7vTb4OzP1+by9Pj2+PrM1OTv8vfR2Ofo7PNNak5MAAAC/klEQVR4nO2cgXqCMAyEW0BAQX3/t90qYw61JVcKuTL+J8h9Jbk0jRpzcHBwcLB3+qJtS+0gFtK3TVfbbxrtSBbQF3c7ku+B9KfOPrlqhxPJ7WSn9NoRRdF29pVWO6YIyjcVjrN2WChF/VGHtYV2ZBD9xSMjs/rb+mV801Xa8Ulpgjqy+bwqX3b8IQdb7OdlZJEoZ5EOfouXnUcGZyLIjxFqk//s5h60gw2B6GAuXWEjzOhIrpgQ3p4eSHUH7bclr70DtBVYaoYjF+2AfRT/VUitHbAPyA4PIRtwCGHjEMLGboTc9iIENcROO2Afu2lR0E+LVgjaxtMKQdt42vvIbi5WkqnvLoWctAP2AgqhnTWCYy3bcM6D0A+L9kzQmuWgzJKYE+Ec0UUI4XxMDDxJ++DM9tnH3He0Q/4MekGkvVmh1xHaF1G8/lJWXwM+vDko/dBECLlpR+zhfc9sBs7qizeNpEXLoE5SE2/TQebOmiGO16XSIMz7Z4gn0s4ZHYgn0o4eHgAVmLOFHwGSRDvUMPLxL3WKGKBNYe2zRu7zEgaYi69DWrdoJ/G/COsWd81yyDyRPdUdon6LtYGfINDBesedIjBF7RBlzGcJ741qwry789feB/NCcqhZRlSAtUOUIRidaocoYzdVSzBLycIPJdbOPEB5Imgb2S8jA/M68mhRKoEQ7gnKg6qQ/IikLonHpQPiMQr7VVf8ox72IxE/trPfdaU67F070jDyKTb5nUQ81mLvt/byE1dkz5T5muj9x4qPkC7QGdPCe04NoZlU+EqN40JmJ33EitMPNVHSFxErZ38pOZKljFgBfOWqnizn+G9qSqe6p1nAazQhtMpxH1enQmjUsNvCBPdQnza9cFUpEtxHs9msaIFpyNgm8ZeahozVrQXvp2JZtYahu36kUrAmPQXr3OvBf29JQ3pjwffF05B6uJrexaWknbZAS5fESratVuspifkZWEqS5ckmXh4iUfelVbCeJFowUD+QREcieUpbmyQWr1uyBgSF6wti9iDUNVzcMwAAAABJRU5ErkJggg==";
            public const string DefaultFemale = "data:image/jpeg;base64,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";
            public const string DefaultHeaderPng = "data:image/png;base64,";
            public const int MaximumImage = 5 * 1024 * 1024; // 5mb
            public const string DefaultQrCodeExample = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAA+gAAAPoCAYAAABNo9TkAAAAAXNSR0IArs4c6QAAIABJREFUeF7s28GOLEuyc+fb7//QV+gf0EQaFEtKeC3L/fWYnW61SLcIIvb5z//+7//+7//4HwIIIIAAAggggAACCCCAAAII/CmB/yjof8rf4QgggAACCCCAAAIIIIAAAgj8HwIKuiAggAACCCCAAAIIIIAAAgggECCgoAdMMAICCCCAAAIIIIAAAggggAACCroMIIAAAggggAACCCCAAAIIIBAgoKAHTDACAggggAACCCCAAAIIIIAAAgq6DCCAAAIIIIAAAggggAACCCAQIKCgB0wwAgIIIIAAAggggAACCCCAAAIKugwggAACCCCAAAIIIIAAAgggECCgoAdMMAICCCCAAAIIIIAAAggggAACCroMIIAAAggggAACCCCAAAIIIBAgoKAHTDACAggggAACCCCAAAIIIIAAAgq6DCCAAAIIIIAAAggggAACCCAQIKCgB0wwAgIIIIAAAggggAACCCCAAAIKugwggAACCCCAAAIIIIAAAgggECCgoAdMMAICCCCAAAIIIIAAAggggAACCroMIIAAAggggAACCCCAAAIIIBAgoKAHTDACAggggAACCCCAAAIIIIAAAgq6DCCAAAIIIIAAAggggAACCCAQIKCgB0wwAgIIIIAAAggggAACCCCAAAIKugwggAACCCCAAAIIIIAAAgggECCgoAdMMAICCCCAAAIIIIAAAggggAACCroMIIAAAggggAACCCCAAAIIIBAgoKAHTDACAggggAACCCCAAAIIIIAAAgq6DCCAAAIIIIAAAggggAACCCAQIKCgB0wwAgIIIIAAAggggAACCCCAAAIKugwggAACCCCAAAIIIIAAAgggECCgoAdMMAICCCCAAAIIIIAAAggggAACCroMIIAAAggggAACCCCAAAIIIBAgoKAHTDACAggggAACCCCAAAIIIIAAAgq6DCCAAAIIIIAAAggggAACCCAQIKCgB0wwAgIIIIAAAggggAACCCCAAAIKugwggAACCCCAAAIIIIAAAgggECCgoAdMMAICCCCAAAIIIIAAAggggAACCroMIIAAAggggAACCCCAAAIIIBAgoKAHTDACAggggAACCCCAAAIIIIAAAgq6DCCAAAIIIIAAAggggAACCCAQ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";
            public const int MaximumImageStored = 300 * 1024; // 300kb
        }
        
        public class Auth
        {
            public const string AuthenticationUsername = "Authentication:Username";
            public const string AuthenticationPassword = "Authentication:Password";
            public const string Authorization = "Authorization";
            public const string BasicAuthorization = "Basic Authorization";
            public const string StringType = "string";
            public const string Header = "header";
            public const string Basic = "Basic";
            public const string BearerHeader = "Bearer ";
            public const string BasicHeader = "Basic ";
            public const string Iso88591 = "iso-8859-1";
        }

        public class CommonFields
        {
            public const string CreatedBy = "CreatedBy";
            public const string CreatedOn = "CreatedOn";
            public const string UpdatedBy = "UpdatedBy";
            public const string UpdatedOn = "UpdatedOn";

            public const string IsDeleted = "IsDeleted";
        }
        public class Policy
        {
            public const string SuperAdmin = "SuperAdmin";
            public const string PrimaryAdmin = "PrimaryAdmin";
            public const string SecondaryAdmin = "SecondaryAdmin";
            public const string Employee = "Employee";
            public const string SystemAdmin = "SystemAdmin";
            public const string SystemAndSuperAdmin = "SystemAndSuperAdmin";
            public const string SystemAndSuperAndPrimaryAdmin = "SystemAndSuperAndPrimaryAdmin";
            public const string SuperAndPrimaryAdmin = "SuperAndPrimaryAdmin";
            public const string PrimaryAndSecondaryAdmin = "PrimaryAndSecondaryAdmin";
            public const string PrimaryAndSecondaryAdminAndEmployee = "PrimaryAndSecondaryAdminAndEmployee";
            public const string SuperAndPrimaryAndSecondaryAdmin = "SuperAndPrimaryAndSecondaryAdmin";
            public const string SuperAndPrimaryAndSecondaryAdminAndEmployee = "SuperAndPrimaryAndSecondaryAdminAndEmployee";
            public const string SystemAndSuperAndPrimaryAndSecondaryAdmin = "SystemAndSuperAndPrimaryAndSecondaryAdmin";
            public const string SystemAndSuperAndPrimaryAndSecondaryAdminAndEmployee = "SystemAndSuperAndPrimaryAndSecondaryAdminAndEmployee";
        }
        public class FileConfig
        {
            public const string BaseFolderCardQrExport = "files_export/card_qr/file";
            public const string BaseFolderCardQrExportVisit = "files_export/card_qr/file/visit";
            public const string FileNameRequestLog = "request_log.txt";
        }

        public class DateTimeFormat
        {
            public const string YyyyMMdd = "yyyyMMdd";
            public const string YyyyMMddhhmm = "yyyyMMddHHmm";
            public const string DdMMyyyy = "ddMMyyyy";
            public const string DdMMYyyy = "dd.MM.yyyy";
            public const string YyMMdd = "yyMMdd";
            public const string DdMdYyyy = "ddMMyyyy";
            public const string MmDdYyyy = "MM.dd.yyyy";
            public const string YyyyMmDdKo = "yyyy.MM.dd";
            public const string MmDdYyyyHHmmss = "MM.dd.yyyy HH:mm:ss";
            public const string MmDdYyyyHHmm = "MM.dd.yyyy HH:mm";
            public const string DdMdYyyyFormat = "dd-MM-yyyy";
            public const string YyyyyMdDdFormat = "yyyy-MM-dd";
            public const string YyyyMmDdHhMmSs = "yyyy/MM/dd HH:mm:ss";
            public const string YyyyMmDd = "yyyy/MM/dd";
            public const string YyyyMmDdHhMmSsFfffff = "yyyy/MM/dd HH:mm:ss.FFFFFF";
            public const string YyyyMMddHHmmss = "yyyyMMddHHmmss";
            public const string DdMMyyyyHHmmss = "ddMMyyyyHHmmss";
            public const string DdMMyyyyHH = "ddMMyyyyHH";
            public const string DdMMyyyyHHmm = "ddMMyyyyHHmm";
            public const string MMddyyyyHHmmss = "MMddyyyyHHmmss";
            public const string DdMMyyyyHHmmssFfffff = "ddMMyyyyHHmmssFFFFFF";
            public const string DatetimeServerFormat = "yyyy/MM/dd HH:mm:ss";
            public const string Hhmm = "hhmm";
            public const string HHmmDot = "HH.mm";
            public const string DdMMyyyyHHmmssFff = "ddMMyyyyHHmmssFFF";
            public const string ddMMyyyyHHmmsszzz = "ddMMyyyyHHmmsszzz";
            public const string HHmmss = "HH:mm:ss";
            public const string HHmm = "HH:mm";
            public const string MMYYYY = "MM/yyyy";
            public const string YYYYMD = "yyyy-MM-dd-THH:mm:ssZ";
            public const string HHmmddMMyyyy = "HHmmddMMyyyy";
        }

        /// <summary>
        /// class define all route in system
        /// </summary>
        public class Route
        {
            public const string EBKNWebsocket = "/ebkn/ws";
            public const string ApiEBKNSetUser = "/ebkn/set-user";
            public const string ApiEBKNGetUser = "/ebkn/get-user";
            public const string ApiEBKNGetFace = "/ebkn/get-face";
            public const string ApiEBKNGetFinger = "/ebkn/get-finger";
            public const string ApiEBKNSetTimeZone = "/ebkn/set-timeZone";

            public const string ApiVerifyLicenseKey = "/verify-license-key";
            public const string ApiVerifyLicenseKeyTest = "/test/verify-license-key";

            public const string ApiAccounts = "/accounts";
            public const string ApiAccountProfile = "/accounts-profile";
            // public const string ApiAccountProfileId = "/accounts-profile/{id}";
            public const string ApiAccountsId = "/accounts/{id}";
            public const string ApiAccountsAvatar = "/accounts/avatar";
            public const string ApiResetPassword = "/accounts/reset-password";
            public const string ApiForgotPassword = "/accounts/forgot-password";
            public const string ApiChangePassword = "/accounts/change-password";
            public const string ApiChangePasswordNoLogin = "/accounts/change-password-no-login";
            public const string ApiGetAccountsType = "/accounts/type";
            public const string ApiGetPrimaryAccounts = "/accounts/primary";
            public const string ApiGetApprovalAccounts = "/accounts/approval";
            public const string ApiAccountsListTimeZone = "/accounts/get-timezone-by-standard";
            public const string ApiAccountsListSystem = "/accounts/get-preferred-system";
            public const string ApiUpdateTimeZoneByAccounts = "/accounts/update-timezone";
            public const string ApiRefreshToken = "/refreshToken";
            public const string ApiSwitchCompany = "/switch-company";
            public const string ApiResetDefaultTimezone = "/accounts/reset-timezone";
            public const string ApiGetAccessApprovalAccounts = "/accounts/access-approval";
            public const string ApiLastLogin = "/accounts/last-login";
            public const string ApiGetTokenWithUser = "/accounts/get-token";
            public const string ApiAccountCountReview = "/accounts/waiting-for-review";
            public const string ApiAccountTokenInfo = "/accounts/token-info";
            public const string ApiRootAccount = "/accounts/root";

            public const string ApiGetVisitApprovalAccounts = "/accounts/visit/approval";
            public const string ApiGetUserApprovalAccounts = "/accounts/user/approval";

            public const string ApiAccessGroups = "/access-groups";
            public const string ApiAccessGroupsId = "/access-groups/{id}";
            public const string ApiAccessGroupsDoors = "/access-groups/{id}/doors";
            public const string ApiAccessGroupsUsersList = "/access-groups/{id}/users";
            public const string ApiAccessGroupsAssignDoors = "/access-groups/{id}/assign-doors";
            public const string ApiAccessGroupsAssignUsers = "/access-groups/{id}/assign-users";
            public const string ApiAccessGroupsUnAssignedDoors = "/access-groups/{id}/unassigned-doors";
            public const string ApiAccessGroupsUnAssignedUsers = "/access-groups/{id}/unassigned-users";
            public const string ApiAccessGroupsUnAssignUser = "/access-groups/{id}/unassign-user/{userId}";
            public const string ApiAccessGroupsUnAssignUsers = "/access-groups/{id}/unassign-users";
            public const string ApiAccessGroupsUnAssignDoor = "/access-groups/{id}/unassign-door/{doorId}";
            public const string ApiAccessGroupsUnAssignDoors = "/access-groups/{id}/unassign-doors";
            public const string ApiUnAssignDoorsAllAccessGroup = "/access-groups/all/unassign-doors";
            public const string ApiAccessGroupsChangeTimezones = "/access-groups/{id}/change-timezones";
            public const string ApiAccessGroupsChangeTimezone = "/access-groups/{id}/change-timezone";
            public const string ApiAddAccessGroupWithAccessTimeNames = "/access-groups/access-times";

            public const string ApiAcGrpDeptId = "/access-groups/{id}/departments";
            public const string ApiAssignDeptToAcGrpId = "/access-groups/{id}/assign-departments";
            public const string ApiUnassignDeptToAcGrpId = "/access-groups/{id}/unassign-departments";

            // for test
            public const string ApiAccessGroupsAllDoorsForVisit = "/access-groups/{id}/all-doors-visits";

            public const string ApiAccessLevels = "/access-levels";
            public const string ApiAccessLevelsDoorId = "/access-levels/{doorId}";
            public const string ApiAccessLevelsDoorList = "/access-levels/{doorId}/copy-settings/door-list";
            public const string ApiAccessLevelsAssignUsers = "/access-levels/{doorId}/{tzId}/assign-users";
            public const string ApiAccessLevelsAssignedUsers = "/access-levels/{doorId}/assigned-users";
            public const string ApiAccessLevelsChangeTimezone = "/access-levels/{doorId}/change-timezone/{tzId}";
            public const string ApiAccessLevelsUnAssignedUsers = "/access-levels/{doorId}/unassigned-users";
            public const string ApiAccessLevelsCopyDoorSettings = "/access-levels/{doorId}/copy-settings";
            public const string ApiAccessLevelsAssignedUsersInit = "/access-levels/{doorId}/assigned-users/init";

            public const string ApiAttendancesInit = "/attendances/init";
            public const string ApiGetAttendanceByCompany = "/attendances";
            public const string ApiEditAttendance = "/attendances/{id}";
            public const string ApiAttendanceAccessHistory = "/attendances/{id}/access-history";
            public const string ApiTypesExportAttendance = "/attendances/export/types";
            public const string ApiGetAttendanceRecordEachUser = "/attendances/users";
            public const string ApiReCheckAttendanceFromEventLog = "/attendances/recheck";
            public const string ApiReCheckAttendanceSetting = "/companies/{id}/attendances/recheck-setting";
            public const string ApiSettingAttendance = "/settings/attendance";
            public const string ApiAddLeaveAttendance = "/attendances/leave-requests/add";
            public const string ApiRegisterLeaveAttendance = "/attendances/leave-requests";
            public const string ApiRegisterLeaveAttendanceUser = "/attendances/my-leave-requests";
            public const string ApiUpdateLeaveAttendance = "/attendances/leave-requests/{id}/review";
            public const string ApiEditLeaveAttendance = "/attendances/leave-requests/{id}";
            public const string ApiLeaveAttendanceInit = "/attendances/leave-init";
            public const string ApiGetAttendanceTypeList = "/attendances/types";
            public const string ApiReCalculateAttendance = "/attendances/recalculate";
            public const string ApiAttendancesByCompanyEveryMonth = "/attendances/report-month";
            public const string ApiAttendanceLeaveReportYear = "/attendances/leave-requests/report-year";
            public const string ApiLeaveRequestHistoryOfUser = "/attendances/users/{id}/leave-requests";
            public const string ApiSettingLeaveRequest = "/settings/leave-request";
            public const string ApiAttendanceReaderSetting = "/attendances/readers";
            public const string ApiAttendanceStartTime = "/attendances/starttime";

            // for army now
            public const string ApiAttendanceLeaveTypes = "/attendances/leave-types";
            public const string ApiSoldierManagementTypes = "/army-users/management-types";
            public const string ApiAttendanceLeave = "/attendances/leave";
            public const string ApiSoldierManagement = "/army-users/management";
            public const string ApiAttendanceLeaveId = "/attendances/leave/{id}";
            public const string ApiSoldierManagementId = "/army-users/management/{id}";

            public const string ApiBuildings = "/buildings";
            public const string ApiBuildingsChild = "/buildings/get-building-tree";
            public const string ApiBuildingsLevel = "/buildings/get-building-level";
            public const string ApiBuildingsId = "/buildings/{id}";
            public const string ApiBuildingsDoorListById = "/buildings/{id}/doors";
            public const string ApiBuildingsDoorList = "/buildings/doors";
            public const string ApiExportBuildingsDoorList = "/buildings/doors/export";
            public const string ApiBuildingsAccessibleDoorList = "/buildings/{id}/accessible-doors";
            public const string ApiBuildingsAssignDoors = "/buildings/{id}/assign-doors";
            public const string ApiBuildingsUnAssignDoors = "/buildings/{id}/unassign-doors";

            public const string ApiBuildingMaster = "/buildings-master";
            public const string ApiExportBuildingMaster = "/buildings-master/export";
            public const string ApiNotBuildingMaster = "/not-buildings-master";
            public const string ApiBuildingMasterId = "/buildings-master/{id}";

            public const string ApiCardRequests = "/card-requests";
            public const string ApiCardTypes = "/cards/list-card-type";
            public const string ApiCardsIdFingerprintTemplate = "/cards/{id}/fingerprint-template";

            public const string ApiCategories = "/categories";
            public const string ApiCategoriesId = "/categories/{id}";
            public const string ApiCaregoriesHierarchy = "/categories/hierarchy";
            public const string ApiCaregoryOptionsHierarchy = "/categories/options/hierarchy";

            public const string ApiCompanies = "/companies";
            public const string ApiCompanyId = "/company/{id}";
            public const string ApiCompaniesId = "/companies/{id}";
            public const string ApiAssignCompany = "/company/{id}/assign-doors";
            public const string ApiCompaniesGetLogo = "/companies/get-logo";
            public const string ApiCompaniesMiniLogo = "/companies/{id}/mini-logo";
            public const string ApiCompaniesUpdateLogo = "/companies/update-logo";
            public const string ApiRegenerateCompanyKey = "/companies/{id}/reset-aes-key";
            public const string ApiRegenerateCompaniesKey = "/companies/reset-aes-key";
            public const string ApiGetPlugByCompany = "/companies/{id}/plugins";
            public const string ApiCompaniesIdDetail = "/companies/{id}/detail";
            public const string ApiCompaniesIdAssignUnregisteredDevice = "/companies/{id}/assign-unregistered-device";
            public const string ApiCompanyResetAllowIp = "/company/{id}/Reset-allow-ip";

            public const string ApiEnableDBEncrption = "/companies/{id}/encrypt";
            public const string ApiEnableExpiredPW = "/companies/{id}/expired-pw";
            public const string ApiGetCreditCompany = "/credits/{id}";
            public const string ApiCredits = "/credits";
            public const string ApiUpdatePointCredit = "/credits/point";
            public const string ApiTransaction = "/transactions";
            public const string ApiTransactionInit = "/transactions/init";

            public const string ApiEncryptTest = "/test/enc";
            public const string ApiDecryptTest = "/test/dec";

            public const string ApiCanteenMealTypes = "/meal-types";
            public const string ApiCanteenMealTypeId = "/meal-types/{id}";
            public const string ApiCanteenMealTypeCodeList = "/meal-types/code-list";

            public const string ApiCanteenMealEventLogsId = "/meal-event-logs/{Id}";

            public const string ApiDepartments = "/departments";
            public const string ApiDepartmentsId = "/departments/{id}";
            public const string ApiDepartmentsIdAssign = "/departments/{id}/assign";
            public const string ApiDepartmentsIdUnAssign = "/departments/{id}/un-assign";
            public const string ApiDepartmentsUsersUnAssign = "/departments/{id}/users-un-assign";
            public const string ApiDepartmentsNumber = "/departments/number";
            public const string ApiDepartmentsImport = "/departments/import";
            public const string ApiDepartmentsExport = "/departments/export";
            public const string ApiDepartmentsExportPdf = "/departments/exportpdf";
            public const string ApiDepartmentsHierarchy = "/departments/hierarchy";
            public const string ApiGetListParentDepartment = "/departments/parent-department";
            public const string ApiDepartmentsChild = "/departments/get-department-tree";

            public const string ApiDeviceInit = "/device-init";
            public const string ApiDevices = "/devices";
            public const string ApiDevicesList = "/devices/device-list";
            public const string ApiValidDevices = "/devices/valid";
            public const string ApiDeviceTypes = "/devices/list-device-type";
            public const string ApiStopProcess = "/devices/stop-process";
            public const string ApiDeviceUploadFile = "/devices/upload-file";
            public const string ApiReinstallDevices = "/devices/reinstall-devices";
            public const string ApiUnregitedDevices = "/devices/detect-new-devices";
            public const string ApiUnregisteredDevices = "/devices/unregistered-devices";
            public const string ApiDeviceInstruction = "/devices/send-device-instruction";
            public const string ApiAddMissingDevices = "/devices/add-missing-devices";
            public const string ApiTransmitInfo = "/devices/transmit-info";
            public const string ApiTransmitData = "/devices/transmit-data";
            public const string ApiRequestOpenDoor = "/devices/request/open";
            public const string ApiDevicesMasterCard = "/devices/master-card";
            public const string ApiDeviceInitialize = "/devices/initialize";
            public const string ApiDeviceGetFilter = "/devices/list-filter";
            public const string ApiDeviceGetInit = "/devices/init";
            public const string ApiDeviceConfigLocalMqtt = "/devices/config/local-mqtt";
            public const string ApiDeviceConfigActiveFaceLicense = "/devices/config/active-face-license";
            public const string ApiDevicesAlarm = "/devices/config/alarm";
            public const string ApiDevicesSyncUserFromDevice = "/devices/{id}/sync-user-from-device";
            public const string ApiDevicesGrpcAddIpManual = "/devices/grpc/add-ip-manual";
            public const string ApiDevicesGrpcReconnect = "/devices/{id}/grpc/reconnect";
            public const string ApiDevicesFileLogs = "/devices/file-logs";
            public const string ApiDevicesIdFileLogs = "/devices/{id}/file-logs";
            public const string ApiDevicesIdRequestFileLogs = "/devices/{id}/request-file-logs";

            public const string ApiDevicesId = "/devices/{id}";
            public const string ApiDeviceInfo = "/devices/{id}/update-device-info";
            public const string ApiCopyDevices = "/devices/{id}/copy-device-settings";
            public const string ApiDeviceUpdateStatus = "/devices/{id}/toggle-status";
            public const string ApiCheckUserSetting = "/devices/{id}/check-user-info";
            public const string ApiCheckUserSettingCompare = "/devices/{id}/check-user-info/compare";
            public const string ApiCheckDeviceSetting = "/devices/{id}/check-device-info";
            public const string ApiCheckHolidaySetting = "/devices/{id}/check-holiday-info";
            public const string ApiCheckTimezoneSetting = "/devices/{id}/check-timezone-info";
            public const string ApiDevicesAccessibleUsers = "/devices/{id}/accessible-user";
            public const string ApiDevicesAccessibleUsersByDevices = "/devices/accessible-user";
            public const string ApiDevicesUnAccessibleUsers = "/devices/{id}/unaccessible-user";
            public const string ApiDevicesUserInfoExport = "/devices/{id}/export-user-info";
            public const string ApiDevicesAccessibleUsersExport = "/devices/{id}/accessible-user/export";
            public const string ApiMultiDevicesAccessibleUsersExport = "/devices/accessible-user/export";
            public const string ApiDeviceHistory = "/devices/{id}/history";
            public const string ApiReUpdateUpTimeDevice = "devices/recheck-uptime";
            public const string ApiReUpdateUpTimeDeviceById = "devices/{id}/recheck-uptime";
            public const string ApiDevicesUsersInfo = "/devices/{id}/user-info";
            public const string ApiDevicesCondition = "/devices/condition/{mprId}";
            public const string ApiDevicesUserMasterCard = "/devices/{id}/user-master-card";
            public const string ApiDevicesUsersInfoByCardId = "/devices/{id}/user-info-by-cardid";
            public const string ApiDeviceFaceCaptureRequest = "/devices/{id}/face-capture";
            public const string ApiDeviceTokenSubDisplayInfo = "/devices/token/sub-display-info";
            public const string ApiDeviceSubDisplayInfo = "/devices/sub-display-info";
            public const string ApiDeviceAssignUsers = "/devices/{id}/assign-users";
            public const string ApiDeviceUnassignUsers = "/devices/{id}/unassign-users";

            public const string ApiUserInfoByCardId = "/devices/user-info-by-cardid";
            public const string ApiPinToDevice = "/devices/{id}/pinpad";

            public const string ApiDeviceMessage = "/device-message";
            public const string ApiUpdateDeviceMessage = "/device-message/{id}";

            public const string ApiDevicesRegisteredCardsByTypes = "/devices/{id}/registered-card";

            public const string ApiControllerDevices = "/controller-devices";
            public const string ApiControllerDevicesId = "/controller-devices/{id}";

            public const string ApiAssignControllerDevicesId = "/controller-devices/{id}/assign";

            public const string ErrorPage = "/error/systemerror";
            public const string NotFoundPage = "/error/pagenotfound";
            public const string AccessDeniedPage = "/error/accessdenied";

            public const string ApiEventLogs = "/event-logs";
            public const string ApiEventLogsId = "/event-logs/{id}";
            public const string ApiEventLogsInit = "/event-logs/init";
            public const string ApiEventLogsReport = "/event-logs/report";
            public const string ApiEventLogsExport = "/event-logs/export";
            public const string ApiEventLogsRecovery = "/event-logs/recovery";
            public const string ApiEventLogsExportPdf = "/event-logs/exportpdf";
            public const string ApiEventLogsOpenReport = "/event-logs/open/report";
            public const string ApiEventLogsReportInit = "/event-logs/report/init";
            public const string ApiEventLogsEventInquiry = "/event-logs/inquiry";
            public const string ApiEventLogsReportExport = "/event-logs/Report/export";
            public const string ApiEventLogsRecoveryDevices = "/event-logs/recovery-list";
            public const string ApiEventLogIdBodyTemperature = "/event-logs/{id}/body-temperature";
            public const string ApiEventLogListFileBackup = "/event-logs/backup/files";
            public const string ApiEventLogAccessStatistics = "event-logs/access-statistics";
            public const string ApiEventLogMonitoring = "/event-logs/screen-monitoring";
            public const string ApiEventLogMonitoringToSchool = "/event-logs/screen-monitoring-school";
            public const string ApiEventLogsRelated = "/event-logs/related";
            public const string ApiEventLogsWorkTypeCount = "/event-logs/worktype-count";
            public const string ApiEventLogsWorkType = "/event-logs/worktype";
            public const string ApiInOutStatusUser = "/event-logs/in-out-status/user";
            public const string ApiInOutStatusVehicle = "/event-logs/in-out-status/vehicle";
            public const string ApiInOutStatusUnitVehicle = "/event-logs/in-out-status/unit-vehicle";
            public const string ApiEventMemo = "/event-memo";
            public const string ApiEventMemoId = "/event-memo/{id}";
            public const string ApiEventMemoByEventLogId = "/event-memo/log/{id}";

            public const string ApiEventLogsVisitTypeCount = "/event-logs/visittype-count";
            public const string ApiEventLogsVisitType = "/event-logs/visittype";

            public const string ApiEventLogRidImages = "/event-logs/{rid}/images";
            public const string ApiEventLogRidImagesCheckinVisit = "/event-logs/{rid}/images-checkin-visit";
            public const string ApiEventLogImages = "/event-logs/images";
            public const string ApiEventLogVideos = "/event-logs/videos";
            public const string ApiEventLogRecordVideos = "/event-logs/record-videos";
            public const string ApiImageStatic = "/static/{rootFolder}/{companyCode}/{date}/{fileName}";
            public const string ApiImageStaticCategory = "/static/{rootFolder}/{companyCode}/{category}/{fileName}";
            public const string ApiFaceUploadStatic = "/face/upload/{cameraId}/{year}/{month}/{date}/{fileName}";

            public const string ApiEventLogHVQY = "/event-logs/hvqy";

            //public const string ApiVehicleEventLog = "/vehicle-eventlog";
            public const string ApiVehicleEventLog = "/vehicles-eventlog";
            //public const string ApiVehicleEventLogReport = "/vehicle-eventlog/report";
            public const string ApiVehicleEventLogReport = "/vehicle-eventlog";
            public const string ApiVehicleEventLogReportInit = "/vehicle-eventlog/report/init";

            public const string ApiVehicleEventLogExport = "/vehicle-eventlog/export";

            public const string ApiVehicleAllocation = "/vehicle-allocation";
            public const string ApiVehicleAllocationId = "/vehicle-allocation/{id}";
            public const string ApiVehicleAllocationImport = "/vehicle-allocation/import";

            public const string ApiVehicleAllocationStart = "/vehicle-allocation/{id}/start-allocation-service";
            public const string ApiVehicleAllocationEnd = "/vehicle-allocation/{id}/end-allocation-service";

            public const string ApiUnitVehicleStatus = "/unit-vehicle/status";

            public const string ApiHolidays = "/holidays";
            public const string ApiHolidaysId = "/holidays/{id}";

            public const string ApiDeviceReaders = "/device-readers";
            public const string ApiDeviceReadersExport = "/device-readers/export";
            public const string ApiDeviceReadersId = "/device-readers/{id}";
            public const string ApiMonitors = "/monitors";
            public const string ApiMonitorsDoorList = "/monitors/devices";

            public const string ApiRoles = "/roles";
            public const string ApiRolesId = "/roles/{id}";
            public const string ApiDefaultRoles = "/roles/default";
            public const string ApiChangeRoleSettingDefault = "roles/{id}/user/default-setting";

            public const string ApiSettings = "/settings";
            public const string ApiSettingsId = "/settings/{id}";
            public const string ApiSettingsLocalMqtt = "/settings/local-mqtt";
            public const string ApiSettingsPassword = "/settings/password";

            public const string ApiUsersSettingFirstApprover = "/settings/user/first-approver";
            public const string ApiUsersSettingSecondApprover = "/settings/user/second-approver";

            public const string ApiAddAccountRabbitmq = "/mqtt/add-account";
            public const string ApiDeleteAccountRabbitmq = "/mqtt/delete-account";

            public const string ApiHeaderSettings = "/header";

            public const string ApiSystemLogs = "/system-logs";
            public const string ApiSystemLogAll = "/system-logs-all";
            public const string ApiSystemLogsExport = "/system-logs/export";
            public const string ApiSystemLogsExportPdf = "/system-logs/exportpdf";
            public const string ApiSystemLogsTypeListItems = "/system-logs/type-list";
            public const string ApiSystemLogsActionListItems = "/system-logs/action-list";

            public const string ApiAccessTimes = "/access-times";
            public const string ApiAccessTimesHvqy = "/access-times-hvqy";
            public const string ApiAssignAccessTimesHvqy = "/assign-access-times-hvqy";
            public const string ApiUnAssignAccessTimesHvqy = "/unassign-access-times-hvqy";
            public const string ApiAccessTimesId = "/access-times/{id}";

            public const string ApiUsersInit = "/users/init";
            public const string ApiUsers = "/users";
            public const string ApiUsersReturnUserModel = "/users/return-user-model";
            public const string ApiUsersDeptAssign = "/users-dept-assign";
            public const string ApiExportUsersDeptAssign = "/users-dept-assign/export";
            // I will apply below source code with FE implement.
            public const string ApiUsersDeptUnassign2 = "/users-dept-unassign";
            public const string ApiUsersDeptUnassign = "/users-assign";
            public const string ApiUsersMultiple = "/users-multi";
            public const string ApiUsersSync = "/users-sync";
            public const string ApiUsersSend = "/users-send";
            public const string ApiUsersId = "/users/{id}";
            public const string ApiUsersAddAccount = "/users/add-account-to-user";
            public const string ApiUsersIdVehicles = "/users/{id}/vehicles";
            public const string ApiVehicles = "/vehicles";
            public const string ApiVehiclesImport = "/vehicles/import";
            public const string ApiGetVehicleTemplate = "/vehicles/import-template";
            public const string ApiExportVehicle = "/vehicles/export";
            public const string ApiVehicleId = "/vehicles/{id}";
            public const string ApiUsersIdAvatar = "/users/{id}/avatar";
            public const string ApiUsersTest = "/users/reboot-test";
            public const string ApiUsersImport = "/users/import";
            public const string ApiUsersExport = "/users/export";
            public const string ApiUsersCardCount = "/users/count/{id}";
            public const string ApiUsersEditCards = "/users/{id}/edit-cards";
            public const string ApiUsersImportProjectD = "/users/import/projectd";
            public const string ApiUsersExportProjectD = "/users/export/projectd";
            public const string ApiUsersAccessibleDoors = "/users/{id}/accessible-doors";
            public const string ApiUsersAccessibleDoorsWithUser = "/users/accessible-doors";
            public const string ApiUsersAccessibleDoorsExport = "/users/{id}/accessible-doors/export";
            public const string ApiUserIdentification = "/users/{id}/identification";
            public const string ApiCardByUser = "/users/{id}/identification/{cardId}";
            public const string ApiAccessHistoryUser = "/users/{id}/access-history";
            public const string ApiAccessHistoryEachUser = "/users/access-history";
            public const string ApiGetDynamicQrByUser = "/my-qrcode";
            public const string ApiGetDynamicQrByEmail = "users/qrcode";
            public const string ApiNFCPhoneIdByUser = "/my-phone-id";
            public const string ApiCardListByUser = "/my-card-list";
            public const string ApiReportProblem = "/report-problem-device";
            public const string ApiReportProblemTypes = "/report-problem-device/types";
            public const string ApiUsersByUserCode = "/users-usercode";
            public const string ApiUsersByCondition = "/users/conditions";
            public const string ApiUpdateListAvatarUser = "/users/update-list-avatar";
            public const string ApiUsersSscSchoolSyncUser = "/users/ssc-school/sync-user";

            public const string ApiUserInOutStatus = "/users/in-out-status";
            public const string ApiMultipleUserHVQY = "/users-hvqy";

            public const string ApiRegisterUser = "/register-user";

            public const string ApiUsersRemoveQR = "/remove-qr";
            public const string ApiUsersExpired = "/users-expired";

            public const string ApiApproveUser = "/users/{id}/approved";
            public const string ApiGetLengthUsersReview = "/users/waiting-for-review";
            public const string ApiAccessSetting = "/settings/access";
            public const string ApiAccountApprovalAccessSetting = "/settings/access/approved";

            public const string ApiUnitVehicle = "/unit-vehicles";
            public const string ApiUnitVehicleId = "/unit-vehicles/{id}";

            public const string ApiInitUserVehicle = "/user-vehicles/init";
            public const string ApiUserVehicle = "/user-vehicles";
            public const string ApiUserVehicleId = "/user-vehicles/{id}";

            public const string ApiUserArmys = "/army-users";
            public const string ApiUserArmysDeptAssign = "/army-users-dept-assign";
            public const string ApiUserArmysDeptUnassign = "/army-users-dept-unassign";
            public const string ApiUserArmysId = "/army-users/{id}";
            public const string ApiUserArmysImport = "/army-users/import";
            public const string ApiUserArmysExport = "/army-users/export";
            public const string ApiArmyUserIdentification = "/army-users/{id}/identification";

            public const string ApiArmyInfo = "/army-info";

            public const string ApiUserArmyReviewCount = "/army-users/waiting-for-review";

            public const string ApiVisits = "/visits";
            public const string ApiMyVisits = "/my-visits";
            public const string ApiVisitsUsersTarget = "/visits/users-target";
            public const string ApiVisitsDepartmentsTarget = "/visits/departments-target";
            public const string ApiRegisterVisit = "/register-visit";
            public const string ApiRegisterVisitUsersTarget = "/register-visit/users-target";
            public const string ApiRegisterVisitSetting = "/register-visit/setting";
            public const string ApiQRCodeSMS = "/visit-qrcode";
            public const string ApiVisitSendMailInvite = "/visits/send-mail-invite";
            public const string ApiVisitsId = "/visits/{id}";
            public const string ApiVisitsIdVehicles = "/visits/{id}/vehicles";
            public const string ApiVisitsAccessHistory = "/visits/{id}/access-history";
            public const string ApiVisitsInit = "/visits/init";
            public const string ApiVisitsReport = "/visits/report";
            public const string ApiVisitsReportInit = "/visits/report/init";
            public const string ApiVisitsChangeStatus = "/visits/change-status";
            public const string ApiVisitsPreRegister = "/visits/pre-register";
            public const string ApiVisitsPreRegisterInit = "/visits/pre-register/init";
            public const string ApiVisitsExport = "/visits/export";
            public const string ApiVisitsReportExport = "/visits/report/export";
            public const string ApiVisitsSetting = "/settings/visit";
            public const string ApiVisitsSettingVisible = "/settings/visit-visible";
            public const string ApiVisitIdentification = "/visits/{id}/identification";
            public const string ApiVisitIdentificationMulti = "/visits/identification/multi";
            public const string ApiVisitQR = "/visits/{id}/send-qrcode";
            public const string ApiGetLengthVisitsReview = "/visits/waiting-for-review";
            public const string ApiApprovedVisitor = "/visits/{id}/approved";
            public const string ApiApprovedVisitorMulti = "/visits/approved/multi";
            public const string ApiRejectVisitor = "/visits/{id}/reject";
            public const string ApiReturnCard = "/visits/return";
            public const string ApiReturnCardMulti = "/visits/return/multi";
            public const string ApiAssignedDoorVisitor = "/visits/{id}/assigned-door";
            public const string ApiHistoryVisitor = "/visits/{id}/history";
            public const string ApiVisitorImport = "/visits/import";
            public const string ApiGetVisitorTemplate = "/visits/import-template";
            public const string ApiWebhookData = "/visits/webhook-data";
            public const string ApiVisitAssignDoors = "/visits/{id}/assign-doors";
            public const string ApiVisitUnAssignDoors = "/visits/{id}/unassign-doors";
            public const string ApiVisitsForced = "/visits/forced";
            public const string ApiVisitsIdForced = "/visits/forced/{id}";
            public const string ApiVisitsQrCode = "visits/qrcode";
            public const string ApiGetInfoVisitArmyByCardId = "/visits/all-info";
            public const string ApiVisitAccidentHandleCard = "/visits/{id}/accident-type";
            public const string ApiVisitGetInfoIdentity = "/visits/ocr/info-identity";
            public const string ApiAnonymousGetVisitByPhone = "/visits/get-by-phone";
            public const string ApiRegisterVisitAuthentication = "/register-visit/authentication";
            public const string ApiVisitsImportMulti = "/visits/import-multi";

            public const string ApiVisitsByCardIds = "/visits/cardIds";
            public const string ApiVisitCardById = "/visits/card";

            public const string ApiVisitsSettingFirstApprover = "/settings/visit/first-approver";
            public const string ApiVisitsSettingSecondApprover = "/settings/visit/second-approver";
            public const string ApiVisitsSettingCheckManager = "/settings/visit/check-manager";
            public const string ApiVisitsSettingApplicationManager = "/settings/visit/application-manager";

            public const string ApiVisitArmy = "/army-visits";
            public const string ApiVisitArmyMulti = "/army-visits/multi";
            public const string ApiVisitArmyId = "/army-visits/{id}";
            public const string ApiVisitArmyIdMulti = "/army-visits/{id}/multi";
            public const string ApiVisitArmyReviewCount = "/army-visits/waiting-for-review";
            public const string ApiVisitArmyImportMultiVisitors = "/army-visits/add-multi-visitors";
            public const string ApiVisitArmyExport = "/army-visits/export";
            public const string ApiVisitArmyGetUser = "/army-visits/users";

            public const string ApiVisitArmyTerminate = "/army-visits/terminate";

            public const string ApiWgSettings = "/wgsettings";
            public const string ApiWgSettingsId = "/wgsettings/{id}";
            public const string ApiWgSettingDefaultForIcuDevice = "/wgsettings/default";

            public const string ApiGetWorkingTypeCompany = "/working-types";
            public const string ApiAddWorkingTypeCompany = "/working-types";
            public const string ApiUpdateWorkingTypeCompany = "/working-types/{id}";
            public const string ApiGetWorkingTypeCompanyDetail = "/working-types/{id}";
            public const string AssignMultipleUsersToWorkingTime = "/working-types/{id}/users";
            public const string AssignUserToDeFaultWorkingTime = "/assign/user/working-time/default";


            public const string ApiCornerSetting = "/corners";
            public const string ApiUpdateCornerSetting = "/corners/{id}";
            public const string ApiUserDiscount = "/exceptional-users";
            public const string ApiUserDiscountTypeList = "/exceptional-users/type-list";
            public const string ApiUpdateUserDiscount = "/exceptional-users/{id}";
            public const string ApiMealSetting = "/meal-settings";
            public const string ApiUpdateMealSetting = "/meal-settings/{id}";
            public const string ApiMealSettingHistory = "/meal-settings/{id}/history";
            public const string ApiExceptionalMeal = "/exceptional-meals";
            public const string ApiUpdateExceptionalMeal = "/exceptional-meals/{id}";

            public const string ApiMealServiceTime = "/meal-service-times";
            public const string ApiMealServiceTimeDetail = "/meal-service-times/{id}";


            public const string ApiMealEventLog = "/meal-event-logs";
            public const string ApiMealEventlogEventType = "/meal-event-logs/event-types";
            public const string ApiMealUsageHistory = "/meal-usage-history";
            public const string ApiUpdateMealEventLog = "/meal-event-logs/{id}";
            public const string ApiReportMealEventLog = "/meal-event-logs-reports";
            public const string ApiReportMealEventLogByBuilding = "/meal-event-log-report-buildings";
            public const string ApiReportMealEventLogByCorner = "/meal-event-log-report-corners";
            public const string ApiMealEventLogExportToExcel = "/meal-event-logs-excels";
            public const string ApiMealEventLogExportToExcelUser = "/meal-event-logs-excels/users";


            public const string ApiNotification = "/notifications";
            public const string ApiUpdateNotification = "/notifications/{id}";
            public const string ApiUpdateMultipleNotification = "/update-multiple-notifications";
            public const string ApiDeleteMultipleNotification = "/delete-multiple-notifications";
            public const string ApiDeleteAllNotification = "/delete-all-notifications";
            public const string ApiReadAllNotification = "/read-all-notifications";
            public const string ApiCreateNotification = "/notifications/notice";


            public const string ApiValidationDynamicQrByUser = "/validations/qrcode";

            public const string ApiUploadFile = "/upload/";

            public const string ParameterId = "{id}";

            public const string ApiRoute = "";

            public const string ApiLogin = "/login";
            public const string ApiLogout = "/logout";
            public const string ApiLoginWithCompany = "/login-step2";

            public const string ApiCompanyExpired = "/company-expired";
            public const string ApiCompanyReminder = "/company-reminder";

            public const string ApiPartTimes = "/parttimes";

            public const string ApiCurrentLogo = "/current-logo";
            public const string ApiCurrentQRLogo = "/current-QR-logo";
            public const string ApiEventType = "/event-type";
            public const string ApiCommonSettings = "/common-settings";
            public const string ApiBackupRequest = "/backup-request";
            public const string ApiTestSetSendIcuUserCount = "/set-send-user-count";
            public const string ApiCommonContactManager = "/common-contact";

            public const string ApiInformationVersionSystem = "/versions";

            public const string ApiGetPlugInCompany = "/plugins";
            public const string ApiUpdatePlugInCompany = "/plugins/{id}";


            public const string ApiDashboardTotalCompanies = "/dashboard/total-companies";
            public const string ApiDashboardAdmin = "/dashboard/admin";
            public const string ApiDashboardAccess = "/dashboard/access";
            public const string ApiDashboardCanteen = "/dashboard/canteen";
            public const string ApiDashboardAttendance = "/dashboard/attendance";
            public const string ApiDashboardVisit = "/dashboard/visit";
            public const string ApiDashboardVehicle = "/dashboard/vehicle";
            public const string ApiDashboardTotalOnlineDevices = "/dashboard/total-online-devices";
            public const string ApiDashboardTotalOfflineDevices = "/dashboard/total-offline-devices";
            public const string ApiDashboardRecentlyDisconnectedDevices = "/dashboard/recently-discontected-devices";
            public const string ApiDashboardRecentlyErrors = "/dashboard/recently-errors";
            public const string ApiDashboardNotice = "/dashboard/notice";
            public const string ApiCompanyNotices = "/company-notice";
            public const string ApiCompanyNoticesId = "/company-notice/{id}";

            // [Edward] 2020-05-12
            // API funciton for Duali-Korea
            public const string ApiAttendance = "/duali/attendance";
            public const string ApiADDEventLog = "/ADD/eventLog";

            public const string ApiMailTypes = "/mail-templates/types";
            public const string ApiMailTemplate = "/mail-templates/types/{type}";
            public const string ApiMailTemplateDefault = "/mail-templates-default/{type}";
            public const string ApiMailTemplateUpdateDefault = "/mail-templates/update-default";
            public const string ApiMailTemplateChangeLinkImage = "/mail-templates/update-image";


            public const string ApiCamerasInit = "/cameras/init";
            public const string ApiCameras = "/cameras";
            public const string ApiCameraId = "/cameras/{id}";
            public const string ApiCameraWebhookTsCamera = "/cameras/webhook-ts-camera";
            public const string ApiCameraRecheckEventLog = "/cameras/recheck/event-logs";
            public const string ApiCameraHistory = "/cameras/{id}/history";

            public const string ApiMeetingsInit = "/meetings/init";
            public const string ApiMeetings = "/meetings";
            public const string ApiMeetingId = "/meetings/{id}";
            public const string ApiMeetingIdUsers = "/meeting/{id}/users";
            public const string ApiMeetingReport = "/meetings/{id}/report";
            public const string ApiMeetingExportReport = "/meetings/{id}/report/export";
            public const string ApiMeetingAccessHistory = "/meetings/{id}/access-history";
            public const string ApiMeetingUsersList = "/meetings/{id}/users";
            public const string ApiMeetingVisits = "/meetings/{id}/visits";
            public const string ApiMeetingAssignUsers = "/meetings/{id}/assign-users";
            public const string ApiMeetingAssignVisits = "/meetings/{id}/assign-visits";
            public const string ApiMeetingUnAssignedUsers = "/meetings/{id}/unassigned-users";
            public const string ApiMeetingUnAssignedVisits = "/meetings/{id}/unassigned-visits";
            public const string ApiMeetingUnAssignUsers = "/meetings/{id}/unassign-users";
            public const string ApiMeetingUnAssignVisits = "/meetings/{id}/unassign-visits";

            public const string ApiMeetingRooms = "/meeting-room";
            public const string ApiMeetingRoomId = "/meeting-room/{id}";

            public const string ApiShortenLink = "/shorten-link/{pathname}";

            // [Edward] TEMP API
            // This API is for calling instruction by using RID.
            public const string ApiDeviceInstructionRID = "/devices/send-device-instruction-rid";
            public const string ApiDeviceExistRID = "/devices/exist";

            public const string ApiDeviceDebugMode = "/devices/send-debug-setting";

            public const string ApiElevators = "/elevators";
            public const string ApiElevatorsId = "/elevators/{id}";
            public const string ApiElevatorsAccessGroupsFloors = "/elevators/access-groups/{id}/floors";
            public const string ApiElevatorAccessGroupsUnAssignedFloors = "/elevators/access-groups/{id}/unassigned-floors";
            public const string ApiElevatorsAccessGroupsAssignFloors = "/elevators/access-groups/{id}/assign-floors";
            public const string ApiElevatorsAccessGroupsUnAssignFloors = "/elevators/access-groups/{id}/unassign-floors";

            public const string ApiCallingDoorBell = "/devices/{rid}/door-bell";
            public const string ApiDoorBellRequestCall = "/devices/door-bell/request-call";
            public const string ApiOpenDoorByVideoDoorBell = "/devices/door-bell/open";
            public const string ApiDevicesDoorBellCheckRoomStatus = "/devices/door-bell/check-room-status";

            public const string ApiCardIssueListDevices = "/card-issuing/devices";
            public const string ApiCardIssueDeviceInit = "/card-issuing/devices-init";
            public const string ApiCardIssueDeviceId = "/card-issuing/devices/{id}";
            public const string ApiCardIssueDeviceIdConnect = "/card-issuing/devices/{id}/connect";
            public const string ApiCardIssueDeviceIdTestConnect = "/card-issuing/devices/{id}/test-connect";
            public const string ApiCardIssueCardLayoutInit = "/card-issuing/card-layouts/init";
            public const string ApiCardIssueCardLayouts = "/card-issuing/card-layouts";
            public const string ApiCardIssueCardLayoutId = "/card-issuing/card-layouts/{id}";
            public const string ApiCardLayoutIdPreview = "/card-issuing/card-layout/{id}/preview";
            public const string ApiCardLayoutPreviewFillData = "/card-issuing/card-layout/preview/fill-data";
            public const string ApiCardIssueCards = "/card-issuing/cards";
            public const string ApiCardIssueCardAccidentHandling = "/card-issuing/cards/{id}/accident-handle";
            public const string ApiCardIssueCardId = "/card-issuing/cards/{id}";
            public const string ApiCardIssueCardIdPrint = "/card-issuing/cards/{id}/print";
            public const string ApiCardIssueCardIdIssue = "/card-issuing/cards/{id}/issue";
            public const string ApiCardIssueCardIdRelease = "/card-issuing/cards/{id}/release";
            public const string ApiCardIssueCardInit = "/card-issuing/card-init";
            public const string ApiGetCardByCardId = "/card/get-by-cardId/{cardId}";
            // public const string ApiCardIssueWriteData = "/card-issuing/issue-data/write";
            // public const string ApiCardIssueReadData = "/card-issuing/issue-data/read";
            // public const string ApiCardIssueDeviceIdPrinting = "/card-issuing/devices/{id}/print-by-image";
            public const string ApiSmartCardPrinterIdPrinting = "/card-issuing/smart-card-printer/{id}/print-by-image";
            public const string ApiSmartCardPrinterIdPrintingPreview = "/card-issuing/smart-card-printer/{id}/print-by-image-preview";
            public const string ApiCardIssueExportCards = "/card-issuing/cards/export";
            public const string ApiCardIssueCheckLogMessage = "/card-issuing/check-log";
            public const string ApiCardPrinterIdPrinting = "/card-issuing/card-printer/{id}/print";

            public const string ApiCardIssuingAccidentType = "/card-issuing/accident-type";
            public const string ApiCardIssuingAccidentTypeId = "/card-issuing/accident-type/{id}";
            public const string ApiCardIssuingPersonCardType = "/card-issuing/person-card-type";
            public const string ApiCardIssuingPersonCardTypeId = "/card-issuing/person-card-type/{id}";
            public const string ApiTypesPersonArmy = "/army/person-type";

            public const string ApiGuardHouseRegistrationVehicle = "/guard-house/vehicles";
            public const string ApiGuardHouseRegistrationPerson = "/guard-house/persons";

            public const string ApiSystemMigrationResetRabbitmq = "/system-info/rabbitmq/reset-permission";
            public const string ApiSystemMigrationResetAvatar = "/system-info/user-avatar/reset-link-domain";
            public const string ApiSystemMigrationLinkEventLog = "/system-info/event-log/reset-link-image";

            public const string ApiBooks = "/books";
            public const string ApiBooksExport = "/books/export";
            public const string ApiBooksInit = "/books/init";
            public const string ApiBooksId = "/books/{id}";
            public const string ApiBooksArea = "/books/area";
            public const string ApiBooksAreaId = "/books/area/{id}";
            public const string ApiBooksIdBorrow = "/books/{id}/borrow";
            public const string ApiBooksIdReturn = "/books/{id}/return";
            public const string ApiBooksIdExtend = "/books/{id}/extend";
            public const string ApiBooksMyBorrow = "/books/my-borrow";
            public const string ApiBooksReportBorrow = "/books/report-borrow";
            public const string ApiBooksReportBorrowExport = "/books/report-borrow/export";
            public const string ApiBooksImportFile = "/books/import";

            public const string ApiUnknownPersons = "/unknown-persons";
            public const string ApiUnknownPersonsIdAccessHistory = "/unknown-persons/{id}/access-history";

            public const string ApiFirmwareVersion = "/firmware-version";
            public const string ApiFirmwareVersionId = "/firmware-version/{id}";
            public const string ApiFirmwareVersionDownload = "/firmware-version/download";
            public const string ApiFirmwareVersionDeviceDownload = "/firmware-version/device-download";
            public const string ApiFirmwareVersionIdUpdateDevice = "/firmware-version/{id}/update-device";

            // for department level
            public const string ApiDepartmentDeviceAssigned = "/doors-dept-assign";
            public const string ApiDepartmentDeviceUnAssigned = "/doors-dept-unassign";
            public const string ApiAssignedDeptDevice = "/departments/{id}/assign-doors";
            public const string ApiUnAssignedDeptDevice = "/departments/{id}/unassign-doors";

            public const string ApiVideoIntercomGetReceiverInfo = "/video-intercom/{rid}/get-receiver-info";
            public const string ApiVideoIntercomStartCall = "/video-intercom/{rid}/start-call";
            public const string ApiVideoIntercomEndCall = "/video-intercom/{rid}/end-call";
            public const string ApiVideoIntercomOpenDoor = "/video-intercom/open-door";
            public const string ApiVideoIntercomGetReceiverInfoWithSession = "/video-intercom/get-receiver-info";

            // ssc-sso
            public const string ApiSscLogin = "ssc-login";

            // webhook
            public const string ApiWebhookReceiveEventLog = "/webhook/receive-event-log";
            public const string ApiWebhookReceiveDoorStatus = "/webhook/receive-door-status";

            // access schedule
            public const string ApiAccessSchedulesInit = "/access-schedules/init";
            public const string ApiAccessSchedules = "/access-schedules";
            public const string ApiAccessSchedulesId = "/access-schedules/{id}";
          
            public const string ApiAccessSchedulesUnAssignedUsers = "/access-schedules/{id}/unassigned-users";
            public const string ApiAccessSchedulesIdUsers = "/access-schedules/{id}/users";
           
            
            public const string ApiAccessSchedulesAssignUsers = "/access-schedules/{id}/assign-users";
            public const string ApiAccessScheduleUnAssignUsers = "/access-schedules/{id}/unassign-users";


            // work shift
            public const string ApiWorkShiftsInit = "/work-shifts/init";
            public const string ApiWorkShifts = "/work-shifts";
            public const string ApiWorkShiftsId = "/work-shifts/{id}";
           
           
          
        }

        /// <summary>
        /// Config for swagger
        /// </summary>
        public class Swagger
        {
            public const string Header = "header";
            public const string V1 = "v1";
            public const string V2 = "v2";
            public const string CorsPolicy = "CorsPolicy";
            public const string SwaggerJsonPath = "/swagger/v1/swagger.json";
            public const string MyApiV1 = "Versioned API v1.0";
        }


        public class CornerSetting
        {
            public const string AddSuccess = "Add new corner success";
            public const string AddFailed = "Add new corner failed";
            public const string CheckExistsCornerSetting = "This corner is already existed";
            public const string UpdateSuccess = "Update corner success";
            public const string UpdateFailed = "Update corner failed";
            public const string DeleteSuccess = "Delete corner failed";
            public const string DeleteFailed = "Delete corner failed";
            public const string CornerUsedInMealServiceTime = "This corner is used in a Meal service time!";
        }
        public class MealEventLog
        {
            public const string AddSuccess = "Add new event success";
            public const string EventTimeNotCorrect = "The Event Time is not matched with meal service time setting";
            public const string AddFailed = "Add new event failed";
            public const string UpdateSuccess = "Update event success";
            public const string UpdateFailed = "Update event failed";
            public const string DeleteSuccess = "Delete event failed";
            public const string DeleteFailed = "Delete event failed";
        }

        public class MealSetting
        {
            public const string AddSuccess = "Add new meal setting for corner success";
            public const string AddFailed = "Add new meal setting for corner failed";
            public const string MealSettingForCornerExist = "Meal setting for this meal service for this corner is already exist";
            public const string UpdateSuccess = "Update meal setting for corner success";
            public const string UpdateFailed = "Update meal setting for corner failed";
            public const string DeleteSuccess = "Delete meal setting for corner success";
            public const string DeleteFailed = "Delete meal setting for corner failed";
        }


        public class Notification
        {
            public const string CheckExistsNoti = "Data does not exists";
            public const string UpdateNotiSuccess = "Update Status is Success";
            public const string DeleteSuccess = "Delete Notification is Success";
            public const string DeleteFailed = "Delete Notification is Fail";
            public const string UpdateFailed = "Update Notification is Fail";
            public const string AddFailed = "Add new Notification is Fail";
            public const string AddSuccess = "Add new Notification is Success";

            public const string PushingNotyLeaveRequest = "leave-request"; // pushing notification to employee (leave-request accepted or rejected)
            public const string PushingNotyLeaveRequestReview = "leave-request-review"; // pushing notification to manager (leave-request of employee)
        }
        public class UserDiscount
        {
            public const string AddSuccess = "Add new UserDiscount Success";
            public const string AddFailed = "Add new UserDiscount Failed";
            public const string UpdateSuccess = "Update UserDiscount Success";
            public const string UpdateFailed = "Update UserDiscount Failed";
            public const string DeleteSuccess = "Delete UserDiscount Success";
            public const string DeleteFailed = "Delete UserDiscount Failed";
            public const string UserDiscountOverlap = "Exception User time is overlapped!";
        }
        /// <summary>
        /// Define key for appsetting config 
        /// </summary>
        public class Settings
        {
            public const string FileSettings = "Settings";
            public const string DeviceTypes = "DeviceTypes";
            public const string DefaultCulture = "Cultures:Default";
            public const string OptionCulture = "Cultures:Option";
            public const string PageSize = "Pagination:PageSize";
            public const string LoginExpiredTime = "Login:ExpiredTime";
            public const string ErrorMessage = "ErrorMessage";
            public const string SettingJsonFile = "appsettings.json";
            public const string ConnectionString = "ConnectionString";
            public const string HealthCheckTimeout = "HealthCheck:Timeout";
            public const string DefaultConnection = "DefaultConnection";
            public const string MssqlConnection = "MssqlConnection";
            public const string QueueConnectionSettingsApi = "QueueConnectionSettingsApi";
            public const string DefaultEnvironmentConnection = "ConnectionStrings__DefaultConnection";
            public const string QueueConnectionSettingsHost = "QueueConnectionSettings:Host";
            public const string QueueConnectionSettingsVirtualHost = "QueueConnectionSettings:VirtualHost";
            public const string QueueConnectionSettingsPort = "QueueConnectionSettings:Port";
            public const string QueueConnectionSettingsUserName = "QueueConnectionSettings:UserName";
            public const string QueueConnectionSettingsPassword = "QueueConnectionSettings:Password";
            public const string QueueConnectionSettingsEnableSsl = "QueueConnectionSettings:EnableSsl";
            public const string QueueConnectionSettingsCertPath = "QueueConnectionSettings:CertPath";
            public const string QueueConnectionSettingsCertPassphrase = "QueueConnectionSettings:CertPassphrase";

            public const string QueueConnectionSettingsDeviceCaCertificate = "QueueConnectionSettings:DeviceCaCertificate";
            public const string QueueConnectionSettingsDeviceCientCertificate = "QueueConnectionSettings:DeviceCientCertificate";
            public const string QueueConnectionSettingsDeviceClientKey = "QueueConnectionSettings:DeviceClientKey";

            public const string DeviceCaCertificate = "DeviceCaCertificate";
            public const string DeviceCientCertificate = "DeviceCientCertificate";
            public const string DeviceClientKey = "DeviceClientKey";

            public const string QueueEnvironmentConnectionSettingsDeviceCaCertificate = "QueueConnectionSettings__DeviceCaCertificate";
            public const string QueueEnvironmentConnectionSettingsDeviceCientCertificate = "QueueConnectionSettings__DeviceCientCertificate";
            public const string QueueEnvironmentConnectionSettingsDeviceClientKey = "QueueConnectionSettings__DeviceClientKey";

            public const string QueueEnvironmentConnectionSettingsHost = "QueueConnectionSettings__Host";
            public const string QueueEnvironmentConnectionSettingsVirtualHost = "QueueConnectionSettings__VirtualHost";
            public const string QueueEnvironmentConnectionSettingsPort = "QueueConnectionSettings__Port";
            public const string QueueEnvironmentConnectionSettingsUserName = "QueueConnectionSettings__UserName";
            public const string QueueEnvironmentConnectionSettingsPassword = "QueueConnectionSettings__Password";
            public const string QueueEnvironmentConnectionSettingsEnableSsl = "QueueConnectionSettings__EnableSsl";
            public const string QueueEnvironmentConnectionSettingsCertPath = "QueueConnectionSettings__CertPath";
            public const string QueueEnvironmentConnectionSettingsCertPassphrase = "QueueConnectionSettings_CertPassphrase";
            public const string QueueSettings = "QueueSettings";
            public const string ResourcesDir = "Resources";
            public const string JwtSection = "jwt";
            public const string Cultures = "Cultures";
            public const string DeMasterProCloudDataAccess = "DeMasterProCloud.DataAccess";
            public const string ExpiredCompanyDays = "ExpiredCompanyDays";
            public const string IosApp = "MobileApp:Ios";
            public const string AndroidApp = "MobileApp:Android";
            public const int DefaultExpiredCompanyDays = 60;
            public const int MaxUserSendToIcu = 5;
            public const int MaxUserSendToITouch = 1000;
            public const int DefaultCheckAccessScheduleCronjob = 2;
            public const int NotificationAction = 1;
            public const int NotificationInform = 2;
            public const int NotificationEmergency = 3;
            public const int NotificationWarning = 4;

            public const string DefaultExpiredTime = "60";
            public const string MaxTimezone = "MaxTimezone";
            public const string MaxHoliday = "MaxHoliday";
            public const int ReinstallTimeout = 60000;
            public const int ResponseTimeout = 10000;
            public const int ReinstallStartTimeout = 1500;
            public const int ReinstallTryCount = 3;
            public const int MainFirmwareSendInstruction = 15000;
            public const int DefaultMaxTimezone = 21;
            public const int DefaultMaxHoliday = 32;
            public const string EncryptKey = "Encryptor:Key";
            public const string EncryptIv = "Encryptor:IV";
            public const string MaxCopySettingDoor = "MaxCopySettingDoor";
            public const int DefaultMaxCopySettingDoor = 5;
            public const string DefaultTimezoneTime = "{\"From\":0, \"To\":1439}";
            public const string DefaultTimezoneNotUse = "{\"From\":0, \"To\":0}";
            public const string MaxTimezoneTimeHour = "23";
            public const string MaxTimezoneTimeMinute = "59";
            public const double MaxTimezoneTimeHourMinute = 23.99;
            public const string MaxMprCondition = "MaxMprCondition";
            public const string ExpiredSessionTime = "ExpiredSessionTime";
            public const int DefaultMprDuration = 5;
            public const int DefaultLockOpenDurationSeconds = 3;
            public const int DefaultInOutSet = 2;
            public const string DefaultCompanyName = "DefaultCompany:CompanyName";
            public const string DefaultAccessGroupFullAccess = "DefaultAccessGroup:AccessGroupNameFullAccess";
            public const string DefaultAccessGroupNoAccess = "DefaultAccessGroup:AccessGroupNameNoAccess";
            public const string DefaultAccessGroupVisitAccess = "DefaultAccessGroup:AccessGroupNameVisitAccess";
            public const string DefaultCompanyCode = "DefaultCompany:CompanyCode";
            public const string DefaultContact = "DefaultCompany:Contact";
            public const string DefaultCompanyUsername = "DefaultCompany:Username";
            public const string DefaultCompanyPassword = "DefaultCompany:Password";
            public const string DefaultAccountUsername = "DefaultAccount:Username";
            public const string DefaultAccountPassword = "DefaultAccount:Password";
            public const string DomainFrontEnd = "Domain:FrontEnd";
            public const string MailSettings = "MailSettings";
            public const string MailDevelopSettings = "MailDevelopSettings";
            public const string IcuDefaultValue = "ffffffff";
            //public const string EnablePerformanceLog = "EnablePerformanceLog";
            public const string MaxIcuUser = "MaxIcuUser";
            public const int DefaultMaxIcuUser = 5000;
            public const int DefaultMaxPopUser = 100000;
            public const int NumberTimezoneOfDay = 4;
            public const int NumOfSplitFile = 10;
            public const int MaxSizeSendToIcu = 4 * 1024;
            public const int DefaultVerifyMode = 1;
            public const int DefaultMprInterval = 60;
            public const int DefaultMprAuthCount = 1;
            public const int DefaultSensorDuration = 1;
            public const int DefaultSensorType = 2;
            public const int DefaultBackupPeriod = 3;
            public const int DefaultPositionPassageTimezone = 0;
            public const int DefaultPositionActiveTimezone = 1;
            public const int TimeToLiveLinkDownLoadFw = 1; // 1 hour

            public const int Buzzer_ON = 0;
            public const int Buzzer_OFF = 1;


            public const string MonitoringMaxRecordDisplay = "monitoring_max_record_display";
            public const string MonitoringEventTypeDefault = "monitoring_event_type_default";
            public const string EnablePerformanceLog = "enable_performance_log";
            public const string ProcedureOmission = "procedure_omission";
            public const string PaginationPage = "pagination";
            public const string ValidImageExtension = "valid_image_extension";
            public const string HasNotificationEmail = "notification_email";
            public const string HasNotificationSMS = "notification_SMS";
            public const string ListUserToNotificationEmail = "list_user_to_notification";
            public const string Language = "language";
            public const string Logo = "logo";
            public const string QRLogo = "qr_logo";
            public const string CameraSetting = "camera_setting";
            public const string LocalMqtt = "local_mqtt";
            public const string LocalMqttHost = "local_mqtt_host";
            public const string LocalMqttPort = "local_mqtt_port";
            public const string LocalMqttUserName = "local_mqtt_username";
            public const string LocalMqttPassword = "local_mqtt_password";
            public const string SettingSupport = "setting_support";
            public const string TimePeriodQrcode = "time_period_qrcode";
            public const string AllowGenerateQrCodeOffline = "allow_generate_qrcode_offline";
            public const string UseStaticQrCode = "use_static_qrcode";
            public const string WebhookEventLog = "webhook_eventlog";
            public const string EmployeeEditAvatar = "employee_edit_avatar";
            public const string AutoGenerateQrCode = "auto_generate_qrcode";
            public const string OcrApiKey = "ocr_api_key";
            public const string OcrApiAuthentication = "ocr_api_authentication";
            public const string OcrApiInfoIdentity = "ocr_api_info_identity";
            public const string AutoGenerateNfcPhone = "use_auto_generate_nfc_phone";
            public const string EventListPushNotification = "event_list_push_notification";
            public const string WorkTypeSetting = "work_type_default";
            public const string ArmyWorkTypeSetting = "army_work_type_default";
            public const string AntiPassbackLiftForVisitor = "anti_passback_lift_for_visitor";
            public const string OverwriteFloorLiftForVisitor = "overwrite_floor_lift_for_visitor";
            public const string KeyEmailSupport = "email_support";
            public const string KeyListLanguageOfCompany = "list_language_of_company";
            public const string KeyWhiteListIpAddress = "white_list_ip_address";
            public const string KeyAllowMobileAccessWithoutWhiteList = "allow_mobile_access_without_white_list";
            public const string KeyDevicePassword = "company_device_password";
            public const string KeyChangeInFirstTime = "require_password_change_on_first_login";
            public const string KeyHaveUpperCase = "password_require_uppercase";
            public const string KeyHaveNumber = "password_require_number";
            public const string KeyHaveSpecial = "password_require_special_char";
            public const string KeyTimeNeedToChange = "password_time_need_to_change";
            public const string KeyMaximumTimeUsePassword = "password_max_valid_time";
            public const string KeyMaximumEnterWrongPassword = "max_failed_login_attempts";
            public const string KeyTimeoutWhenWrongPassword = "lockout_duration_minutes";

            public const int LengthCharacterGenQrCode = 7;
            public const int LengthCharacterGenFaceId = 9;
            public const int LengthCharacterGenNFCPhoneId = 10;
            public const string CharacterGenQrCode = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            public const string CharacterGenNfcId = "ABCDEF0123456789";
            public const string NameAccessGroupVisitor = "VAG-";
            public const string NameAccessGroupPersonal = "PAG-";
            public const string PinpadVisitorName = "PINPAD-";
            public const string UpdateUserData = "Update_User_Data_";
            public const int TimeCheckVisitExpired = 24;

            public const string ReCaptChaSiteKey = "GoogleReCaptCha:SiteKey";
            public const string ReCaptChaSecretKey = "GoogleReCaptCha:SecretKey";
            public const string ReCaptChaResponse = "g-recaptcha-response";
            public const string GoogleMapAPI = "GoogleMap:APIKey";

            public const int DefaultMeetingTimeoutAllowed = 15; // 15 min
            public const int DefaultCheckMeetingCronjob = 2; // 2 min

            public const string DefineFolderImages = "images";
            public const string DefineFolderAttendance = "attendance";
            public const string DefineFolderVideos = "videos";
            public const string DefineFolderRecordVideos = "record_videos";
            public const string DefineFolderDBfile = "sqlite";
            public const string DefineFolderNameStoreEventLog = "backup_eventlogs";
            public const string DefineFolderNameStoreSystemLog = "backup_systemlogs";
            public const string DefineFileNameReportMqttError = "report_mqtt_error.csv";
            public const int DefaultCronjobBackupEventLog = 1; // 1 month
            public const string DefineFolderFirmwareVersion = "firmwares";
            public const string DefineFolderDataLogs = "data_logs";

            public const string DefineLimitRangeDayExport = "Limit:Export";
            public const string DefineSettingThreadExport = "ExportEventLogConfig";
            public const string DefineLimitLastCommunicationTime = "Limit:LastCommunicationTime";
            public const string DefineLimitSaveLogOfDevice = "Limit:SaveLogOfDevice";

            public const string DefineConnectionApi = "ConnectionSettingApi:Host";
            public const string DefineConnectionApiforQR = "ConnectionSettingApi:QR_Host";
            
            // reset permission role default
            public const string DefaultRoleEmployee = "[{\"Title\":\"Monitoring\",\"Permissions\":[{\"Title\":\"ViewMonitoring\",\"IsEnabled\":false}]},{\"Title\":\"DeviceMonitoring\",\"Permissions\":[{\"Title\":\"ViewDeviceMonitoring\",\"IsEnabled\":false},{\"Title\":\"SendInstructionDeviceMonitoring\",\"IsEnabled\":false}]},{\"Title\":\"Department\",\"Permissions\":[{\"Title\":\"ViewDepartment\",\"IsEnabled\":false},{\"Title\":\"AddDepartment\",\"IsEnabled\":false},{\"Title\":\"EditDepartment\",\"IsEnabled\":false},{\"Title\":\"DeleteDepartment\",\"IsEnabled\":false},{\"Title\":\"ViewUserDepartment\",\"IsEnabled\":false}]},{\"Title\":\"User\",\"Permissions\":[{\"Title\":\"ViewUser\",\"IsEnabled\":false},{\"Title\":\"AddUser\",\"IsEnabled\":false},{\"Title\":\"EditUser\",\"IsEnabled\":false},{\"Title\":\"DeleteUser\",\"IsEnabled\":false},{\"Title\":\"ExportUser\",\"IsEnabled\":false},{\"Title\":\"SetWorkingTimeUser\",\"IsEnabled\":false}]},{\"Title\":\"DeviceSetting\",\"Permissions\":[{\"Title\":\"ViewDeviceSetting\",\"IsEnabled\":false},{\"Title\":\"EditDeviceSetting\",\"IsEnabled\":false},{\"Title\":\"CopyDeviceSetting\",\"IsEnabled\":false},{\"Title\":\"ReinstallDeviceSetting\",\"IsEnabled\":false},{\"Title\":\"ViewHistoryDeviceSetting\",\"IsEnabled\":false}]},{\"Title\":\"Building\",\"Permissions\":[{\"Title\":\"ViewBuilding\",\"IsEnabled\":false},{\"Title\":\"AddBuilding\",\"IsEnabled\":false},{\"Title\":\"EditBuilding\",\"IsEnabled\":false},{\"Title\":\"DeleteBuilding\",\"IsEnabled\":false}]},{\"Title\":\"Report\",\"Permissions\":[{\"Title\":\"ViewReport\",\"IsEnabled\":false},{\"Title\":\"ExportReport\",\"IsEnabled\":false}]},{\"Title\":\"SystemLog\",\"Permissions\":[{\"Title\":\"ViewSystemLog\",\"IsEnabled\":false},{\"Title\":\"ExportSystemLog\",\"IsEnabled\":false}]},{\"Title\":\"AccessibleDoor\",\"Permissions\":[{\"Title\":\"ViewAccessibleDoor\",\"IsEnabled\":false},{\"Title\":\"ExportAccessibleDoor\",\"IsEnabled\":false}]},{\"Title\":\"RegisteredUser\",\"Permissions\":[{\"Title\":\"ViewRegisteredUser\",\"IsEnabled\":false},{\"Title\":\"ExportRegisteredUser\",\"IsEnabled\":false}]},{\"Title\":\"AccessGroup\",\"Permissions\":[{\"Title\":\"ViewAccessGroup\",\"IsEnabled\":false},{\"Title\":\"AddAccessGroup\",\"IsEnabled\":false},{\"Title\":\"EditAccessGroup\",\"IsEnabled\":false},{\"Title\":\"DeleteAccessGroup\",\"IsEnabled\":false}]},{\"Title\":\"Timezone\",\"Permissions\":[{\"Title\":\"ViewTimezone\",\"IsEnabled\":false},{\"Title\":\"AddTimezone\",\"IsEnabled\":false},{\"Title\":\"EditTimezone\",\"IsEnabled\":false},{\"Title\":\"DeleteTimezone\",\"IsEnabled\":false}]},{\"Title\":\"Holiday\",\"Permissions\":[{\"Title\":\"ViewHoliday\",\"IsEnabled\":false},{\"Title\":\"AddHoliday\",\"IsEnabled\":false},{\"Title\":\"EditHoliday\",\"IsEnabled\":false},{\"Title\":\"DeleteHoliday\",\"IsEnabled\":false}]},{\"Title\":\"VisitManagement\",\"Permissions\":[{\"Title\":\"ViewVisit\",\"IsEnabled\":false},{\"Title\":\"AddVisit\",\"IsEnabled\":false},{\"Title\":\"EditVisit\",\"IsEnabled\":false},{\"Title\":\"DeleteVisit\",\"IsEnabled\":false},{\"Title\":\"ExportVisit\",\"IsEnabled\":false},{\"Title\":\"ViewHistoryVisit\",\"IsEnabled\":false},{\"Title\":\"ApproveVisit\",\"IsEnabled\":false},{\"Title\":\"ReturnCardVisit\",\"IsEnabled\":false}]},{\"Title\":\"VisitReport\",\"Permissions\":[{\"Title\":\"ViewVisitReport\",\"IsEnabled\":false},{\"Title\":\"ExportVisitReport\",\"IsEnabled\":false}]},{\"Title\":\"VisitSetting\",\"Permissions\":[{\"Title\":\"EditVisitSetting\",\"IsEnabled\":false}]},{\"Title\":\"TimeAttendanceReport\",\"Permissions\":[{\"Title\":\"ViewAttendance\",\"IsEnabled\":true},{\"Title\":\"EditAttendance\",\"IsEnabled\":false},{\"Title\":\"ExportAttendance\",\"IsEnabled\":false},{\"Title\":\"ViewHistoryAttendance\",\"IsEnabled\":false}]},{\"Title\":\"WorkingTime\",\"Permissions\":[{\"Title\":\"ViewWorkingTime\",\"IsEnabled\":false},{\"Title\":\"AddWorkingTime\",\"IsEnabled\":false},{\"Title\":\"EditWorkingTime\",\"IsEnabled\":false},{\"Title\":\"DeleteWorkingTime\",\"IsEnabled\":false}]}]";

            public const int TimeoutImageVehicle = 3; // second
            public const int TimeoutVideoVehicle = 5; // second

            public static List<int> ListEventTypeNotUseCameraVideo = new List<int>()
            {
                (short) EventType.CommunicationFailed,
                (short) EventType.CommunicationSucceed
            };
            public static List<string> VisitListFieldsIgnored = new List<string>()
            {
                "id", "gReCaptchaResponse", "covid19", "dynamicQrCode", "processStatus", "accessGroupId", "cardId", "isDecision", "startDateTime", "endDateTime", "statusCode",
                "createdBy", "startTime", "endTime", "buildingAddress", "buildingName", "cardStatus", "leaderId", "visitType", "visiteeAvatar",
                "visiteeDepartmentId", "visiteeId", "autoApproved", "equipments", "sizeRotateAvatar",
            };

            public static int LimitDayStoredNotification = 30; // 30 days

            public static int TimeExpiryDateTokenJitsi = 120; // 120 minutes
            public static int TimeOpenPeriodDoorJitsi = 3; // 3 seconds

            public const string DefineWhiteListDomain = "DomainSetting:WhiteList";

            public const string HeaderIpv4 = "IPv4";

            public const string DateTimeFormatDefault = "dd.MM.yyyy HH:mm:ss";
            public const string DefaultDevicePassword = "DefaultDevicePassword";
            public const string DefineCronjobSetting = "CronjobSetting";

            public static List<object> ListLanguageDefault = new List<object>()
            {
                new { Id = 0, Tag = "en-US" },
                new { Id = 1, Tag = "ja-JP" },
                new { Id = 2, Tag = "ko-KR" },
                new { Id = 3, Tag = "vi-VN" },
            };
            
            public static List<object> ListLanguageCurrent = new List<object>()
            {
                new { Id = 0, Tag = "en-US" },
                new { Id = 1, Tag = "ja-JP" },
                new { Id = 2, Tag = "ko-KR" },
                new { Id = 3, Tag = "vi-VN" },
                new { Id = 4, Tag = "af-NA" },
            };
        }

        public class RabbitMq
        {
            public const string PermissionDefault = "management";
            public const string PermissionTopicDefault = ".*";
            public const string PerifixUserName = "queue";
            
            public const int Scheduler = 120;
            //public const int Scheduler = 600;
            public const string ExchangeName = "amq.topic";
            public const string ExChangeType = "amq.topic";
            public const string EventLogTopic = ".topic.event";
            public const string EventLogJsonTopic = ".topic.event_json";
            public const string EventLogResponseTopic = ".topic.event_log_response";
            public const string EventLogThirdResponseTopic = ".topic.event_third_response";
            public const string EventLogEventCountTopic = ".topic.event_count";
            public const string EventLogEventCountResponseTopic = ".topic.event_count_response";
            public const string EventCountJsonTopic = ".topic.event_count_json";
            public const string EventRecoveryTopic = ".topic.event_recovery";
            public const string EventRecoveryResponseTopic = ".topic.event_recovery_response";
            public const string ConfigurationTopic = ".topic.config";
            public const string ConfigurationResponseTopic = ".topic.config_response";
            public const string CompanyConfig = ".topic.company_config";
            
            public const string CompanyConfigResponse = ".topic.company_config_response";
            public const string AccessControlTopic = ".topic.access_control";
            public const string AccessControlResponseTopic = ".topic.access_control_response";
            public const string HolidayTopic = ".topic.holiday";
            public const string HolidayResponseTopic = ".topic.holiday_response";
            public const string DeviceOnlineTopic = ".topic.online";
            public const string DeviceStatus = ".topic.device_status";
            public const string DeviceInfoResponseTopic = ".topic.device_info_response";
            public const string DeviceInfoTopic = ".topic.device_info";
            public const string DeviceSettingTopic = ".topic.load_device_setting";
            public const string LoadHolidayTopic = ".topic.load_holiday";
            public const string LoadHolidayResponseTopic = ".topic.load_holiday_response";
            public const string LoadTimezoneTopic = ".topic.load_timezone";
            public const string LoadTimezoneResponseTopic = ".topic.load_timezone_response";
            public const string LoadTimezoneWebAppTopic = ".topic.load_timezone_webapp";
            public const string LoadUserTopic = ".topic.load_user";
            public const string LoadUserResponseTopic = ".topic.load_user_response";
            public const string LoadUserWebAppTopic = ".topic.load_user_webapp";
            public const string DeviceSettingResponseTopic = ".topic.load_device_setting_response";
            public const string TimezoneTopic = ".topic.timezone";
            public const string TimezoneResponseTopic = ".topic.timezone_response";
            public const string NotificationTopic = ".topic.notification";
            public const string DeviceInstructionTopic = ".topic.device_instruction";
            public const string DeviceInstructionResponseTopic = ".topic.device_instruction_response";
            public const string EventLogTaskQueue = "event_log_task_queue";
            public const string SetUserTaskQueue = "set_user_task_queue";
            public const string ConvertVideoTaskQueue = "convert_video_task_queue";
            public const string ExportDataToFile = "export_data_to_file";
            public const string RefreshDeviceInfo = "refresh_device_info";
            public const string MultipleMessagesTaskQueue = "multiple_messages_task_queue";
            public const string FileTranferTopic = ".topic.file_transfer";
            public const string FileTranferResponseTopic = ".topic.file_transfer_response";
            public const string LongProcessProgressTopic = ".topic.long_process_progress";
            public const string DeviceMessageTopic = ".topic.message";
            public const string DeviceMessageResponseTopic = ".topic.message_response";
            public const string DeviceSettingWebAppTopic = ".topic.device_setting_webapp";
            public const string HolidayWebAppTopic = ".topic.load_holiday_webapp";
            public const string RequestOpenTopic = ".topic.event.request_open";
            public const string DeviceCommonInstruction = ".topic.device_common_instruction";

            public const string FaceCaptureRequest = ".topic.face_capture";
            public const string FaceCaptureResponse = ".topic.face_capture_response";
            public const string FaceDataToFE = ".topic.face_data";
            public const string FaceRegister = ".topic.register_face";
            public const string FaceRegisterResponse = ".topic.register_face_response";
            
            public const string LFaceRegister = ".topic.register_lface";
            public const string LFaceRegisterResponse = ".topic.register_lface_response";
            public const string TBFaceRegisterResponse = ".topic.register_tbface_response";
            
            public const string DCFaceRegisterResponse = ".topic.register_dcface_response";
            
            public const string TopicBioFaceRegisterResponse = ".topic.register_bioface_response";

            public const string AuthStart = ".topic.auth_start";
            public const string AuthStep1 = ".topic.auth_step1";
            public const string AuthStep2 = ".topic.auth_step2";
            public const string AuthStep3 = ".topic.auth_step3";
            public const string AuthEnd = ".topic.certificate";

            public const string MealServiceTimeTopic = ".topic.meal_service_time";
            public const string MealServiceTimeResponseTopic = ".topic.meal_service_time_response";

            public const string DoorStatusTopic = ".topic.door_status";
            public const string NormalUser = "webapp";
            public const int MaxSendingMessageRetry = 3;
            //Message response timout in milliseconds
            //public const int MessageResponseTimeout = 30000;
            public const int MessageResponseTimeout = 5000;
            public const int MessageResponseInterval = 100;
            public static readonly string Permission = "management";
            public static readonly string DevicePermission = "management";
            public const string TopicPermissionNormalUserList = ".topic.notification.{0}|.topic.notification.process.{0}|.topic.device_status.{0}|.topic.event_json.{0}|.topic.long_process_progress|.topic.event_count_json.{0}|.topic.camera.{0}|.topic.face_data.{0}|.topic.vehicle_event_json.{0}|.topic.load_user_webapp.{0}|.topic.device_scan_finger.*|.topic.device_scan_finger_response.*|.topic.video_call.*|.topic.video_call_response.*";
            public const string TopicPermissionAdminUserList = ".topic.notification.*|.topic.device_status|.topic.event_json.*|.topic.long_process_progress|.topic.load_user_webapp|.topic.event_count_json{0}|.topic.device_setting_webapp|.topic.load_timezone_webapp|.topic.load_holiday_webapp|.topic.video_call.*|.topic.video_call_response.*";
            public const string TopicPermissionDeviceList = ".*";
            public const string TopicPermissionClient = ".topic.sub_display.device_info.*|.topic.event_json.*|.topic.visitor_info.*";

            public const int MaxChannelsPerConnection = 2000;
            public const int Max = 3;
            // Message Time To Live in milisecond
            public const int MessageTLL = 600000; // 10 minutes
            public const int MessageInDeviceTLL = 86400000; // 1 day
            public const int MessageInWebappTLL = 60000; // 60 seconds
            public const int DelayTimeReInstallWhenOnline = 60000; // 60 seconds
            
            public const string SenderIgnoreMessage = "SYSTEM_NOTIFICATION_IGNORE";
            public const string PrefixMsgIdProcess = "PROCESS";
            public const string SenderDefault = "SYSTEM";

            // For nexpa
            public const string lprInformation = ".topic.lpr_information";
            public const string VehicleEventJsonTopic = ".topic.vehicle_event_json";

            public const string FileFromLprTopic = ".topic.camera_images"; // .topic.camera_images.{cameraId}
            public const string SubDisplayDeviceInfo = ".topic.sub_display.device_info"; // .topic.sub_display.device_info.{companyCode}
            public const string RequestUser = ".topic.request_user";
            public const string RequestUserResponse = ".topic.request_user_response";
            public const string NotificationProcessTopic = ".topic.notification.process";
            public const string TopicAccessGroup = ".topic.access_group";
            public const string TopicAccessGroupResponse = ".topic.access_group_response";
            public const string TopicRequestAllUser = ".topic.request_all_user";
            public const string TopicRequestAllUserResponse = ".topic.request_all_user_response";
            public const string TopicSendImageToCardPrint = ".topic.card_printer.print_image";
            public const string TopicSendImageToCardPrintResponse = ".topic.card_printer.print_image_response";
            
            public const string DebugMode = ".topic.debug_mode";
            public const string TopicRequestDeviceUpdateFirmware = ".topic.device_update_firmware";
            public const string TopicVideoCall = ".topic.video_call";
            public const string TopicVideoCallDevice = ".topic.video_call_device";
            public const string TopicVideoCallResponse = ".topic.video_call_response";
            
            // ebkn
            public const string TopicEBKNGetLastEventLog = ".topic.ebkn_get_last_event_log";
            public const string TopicEBKNGetLastEventLogResponse = ".topic.ebkn_get_last_event_log_response";
            public const string TopicEBKNDeleteAllUser = ".topic.ebkn_delete_all_user";
            public const string TopicEBKNAddUserToDevice = ".topic.ebkn_add_user_device";

            // device request
            public const string TopicDeviceRequest = ".topic.device_request";

            public static readonly string[] ListTopicSendToDevice = {
                ".topic.device_info",
                ".topic.access_control",
                ".topic.access_group",
                ".topic.device_instruction",
                ".topic.config",
                ".topic.message",
                ".topic.timezone",
                ".topic.holiday",
            };

            public static readonly string[] ListTopicSendToWebapp =
            {
                ".topic.device_status",
                ".topic.notification.process",
                ".topic.visitor_info",
                ".topic.sub_display.device_info",
                ".topic.vehicle_event_json",
                ".topic.face_data",
                ".topic.load_holiday_webapp",
                ".topic.device_setting_webapp",
                ".topic.notification",
                ".topic.load_user_webapp",
                ".topic.load_timezone_webapp",
                ".topic.event_count_json",
                ".topic.event_json",
                ".topic.camera",
            };
        }

        public class DeliveryMode
        {
            /// <summary>
            /// Delivery mode
            /// </summary>
            public const int QoS0 = 0;
            public const int QoS1 = 1;
            public const int QoS2 = 2;
        }


        /// <summary>
        /// Config for logger system
        /// </summary>
        public class Logger
        {
            public const string LogFile = "Logging:LogDir";
            public const string Logging = "Logging";
            public const string FileDebugChannel = "debug_channel.log";
        }

        public class Pagination
        {
            public const string Start = "start";
            public const string SearchValue = "search[value]";
            public const string OrderColumn = "order[0][column]";
            public const string OrderDirection = "order[0][dir]";
            public const string DefaultOrder = "asc";
        }

        public class ClaimName
        {
            public const string Username = "Username";
            public const string sub = "sub";
            public const string UserCode = "UserCode";
            public const string CompanyCode = "CompanyCode";
            public const string CompanyName = "CompanyName";
            public const string FullName = "FullName";
            public const string DepartmentId = "DepartmentId";
            public const string AccountId = "AccountId";
            public const string AccountType = "AccountType";
            public const string CompanyId = "CompanyId";
            public const string BearerToken = "BearerToken";
            public const string VisitorId = "VisitorId";
            public const string ListDeviceAddress = "ListDeviceAddress";
            public const string QueueService = "QueueService";
            public const string Language = "Language";
            public const string CountDeviceShowing = "CountDeviceShowing";
            public const string IsUseDesign = "IsUseDesign";
            public const string DeviceIds = "DeviceIds";
            public const string Timezone = "Timezone";
            public const string EventType = "EventType";
            public const string EnableDisplayListVisitor = "EnableDisplayListVisitor";
            public const string EnableDisplayAbnormalEvent = "EnableDisplayAbnormalEvent";
            public const string EnableDisplayCanteenEvent = "EnableDisplayCanteenEvent";
            public const string TimeReset = "TimeReset";
            public const string TimeStartCheckIn = "TimeStartCheckIn";
            public const string TimeEndCheckIn = "TimeEndCheckIn";
            public const string TimeStartCheckOut = "TimeStartCheckOut";
            public const string TimeEndCheckOut = "TimeEndCheckOut";
            public const string EnableIgnoreDuplicatedEvent = "EnableIgnoreDuplicatedEvent";
            public const string ParentDepartment = "ParentDepartment";
            public const string IsCheckTeacherOut = "IsCheckTeacherOut";
            public const string ImageBackground5 = "ImageBackground5";
            public const string ColorText = "ColorText";
            public const string JsonData = "JsonData";
            public const string RoomName = "RoomName";
            public const string TemplateCustom = "TemplateCustom";
        }
        public class MailSetting
        {
            public const string DeliveryMethod = "MailSettings:DeliveryMethod";
            public const string From = "MailSettings:From";
            public const string Host = "MailSettings:Host";
            public const string Port = "MailSettings:Port";
            public const string EnableSsl = "MailSettings:EnableSsl";
            public const string DefaultCredentials = "MailSettings:DefaultCredentials";
            public const string UserName = "MailSettings:UserName";
            public const string Password = "MailSettings:Password";
        }

        public class Protocol
        {
            public const string AddUser = "ADD_USER";
            public const string DeleteUser = "DELETE_USER";
            public const string UpdateUser = "UPDATE_USER";
            
            public const string AddVehicle = "ADD_VEHICLE";
            public const string DeleteVehicle = "DELETE_VEHICLE";
            public const string UpdateVehicle = "UPDATE_VEHICLE";

            public const string UpdateDeviceConfig = "UPDATE_DEVICE_CONFIG";
            public const string UpdateDeviceMessage = "UPDATE_DEVICE_MESSAGE";
            public const string DeleteDevice = "DELETE_DEVICE";

            public const string UpdateTimezone = "UPDATE_TIMEZONE";
            public const string DeleteTimezone = "DELETE_TIMEZONE";

            public const string UpdateHoliday = "UPDATE_HOLIDAY";

            public const string Notification = "NOTIFICATION";
            public const string NotificationPageVisitCamera = "PAGE_VISITOR_CAMERA_INFORMATION";
            public const string NotificationCamera = "NOTIFICATION_CAMERA";
            public const string NotificationCameraQrCode = "NOTIFICATION_CAMERA_QRCODE";
            public const string InformationVisitorCamera = "VISITOR_CAMERA_INFORMATION";
            public const string NotificationProcessCamera = "NOTIFICATION_PROCCESS_CAMERA";
            public const string NotificationProcessBook = "NOTIFICATION_PROCESS_BOOK";
            public const string NotificationVisitor = "VISITOR";
            public const string NotificationAction = "ACTION";
            public const string NotificationInform = "INFORM";
            public const string NotificationWarning = "WARNING";
            public const string EventLogType = "EVENT_LOG";
            public const string EventLogResponse = "EVENT_LOG_RESPONSE";
            public const string EventLogEventCount = "EVENT_COUNT";
            public const string EventLogEventRecovery = "EVENT_RECOVERY";
            public const string EventLogEventCountResponse = "EVENT_COUNT_RESPONSE";
            public const string EventCountWebApp = "EVENT_COUNT_WEBAPP";

            public const string VehicleEventLogWebApp = "VEHICLE_EVENT_LOG_WEBAPP";

            public const string LoadDeviceInfo = "LOAD_DEVICE_INFO";
            public const string DeviceInstruction = "DEVICE_INSTRUCTION";

            public const string FileDownLoad = "FILE_DOWNLOAD";
            public const string LongProcessProgress = "LONG_PROCESS_PROGRESS";
            public const string ConnectionStatus = "CONNECTION_STATUS";

            public const string LoadHoliday = "LOAD_HOLIDAY";
            public const string LoadTimezone = "LOAD_TIMEZONE";
            public const string LoadAllUser = "LOAD_ALL_USER";
            public const string LoadAllUserResponse = "LOAD_ALL_USER_RESPONSE";
            public const string LoadUser = "LOAD_USER";
            public const string LoadAllUserWebApp = "LOAD_ALL_USER_WEBAPP";

            public const string LoadHolidayWebApp = "LOAD_HOLIDAY_WEBAPP";
            public const string LoadTimezoneWebApp = "LOAD_TIMEZONE_WEBAPP";

            public const string DoorStatus = "DOOR_STATUS";
            public const string UpdateMealServiceTime = "UPDATE_MEAL_SERVICE_TIME";

            public const string AuthenticationStart = "MUTUAL_AUTHENTICATE_START";
            public const string AuthenticationStep2 = "MUTUAL_AUTHENTICATE_STEP2";
            public const string AuthenticationCertificate = "DOWNLOAD_CERTIFICATE";

            public const string FaceCapture = "FACE_CAPTURE";
            public const string FaceCaptureResponse = "FACE_CAPTURE_RESPONSE";
            public const string SetBioFaceResponse = "SET_BIO_FACE_RESPONSE";

            public const string FaceRegistration = "REGISTER_FACE";
            public const string FaceRegistrationResponse = "REGISTER_FACE_RESPONSE";
            
            public const string LFaceRegister = "REGISTER_LFACE";
            public const string LFaceRegisterResponse = "REGISTER_LFACE_RESPONSE";
            public const string TBFaceRegisterResponse = "REGISTER_TBFACE_RESPONSE";

            public const string UpdateCompanyConfig = "UPDATE_COMPANY_CONFIG";

            // For nexpa
            public const string AddLpr = "ADD_LPR";
            public const string DeleteLpr = "DELETE_LPR";

            public const string EventLogSubDisplayDeviceInfo = "SUB_DISPLAY_DEVICE_INFO";
            public const string RequestUserResponse = "REQUEST_USER_RESPONSE";
            public const string AccessGroupType = "ACCESS_GROUP";

            public const string DebugMode = "DEBUG_MODE";

            public const string RequestAllUser = "REQUEST_ALL_USER";
            public const string RequestAllUserResponse = "REQUEST_ALL_USER_RESPONSE";
            public const string CardPrinterPrintImage = "CARD_PRINTER_PRINT_IMAGE";
            public const string CardPrinterPrintImageResponse = "CARD_PRINTER_PRINT_IMAGE_RESPONSE";
            public const string SetTime = "SET_TIME";
            public const string UpdateDeviceFirmware = "UPDATE_DEVICE_FIRMWARE";
            public const string RejectVideoCall = "REJECT_VIDEO_CALL";
            public const string RejectVideoCallResponse = "REJECT_VIDEO_CALL_RESPONSE";
            public const string HangupVideoCall = "HANGUP_VIDEO_CALL";
            public const string HangupVideoCallResponse = "HANGUP_VIDEO_CALL_RESPONSE";
            public const string RequestVideoCall = "REQUEST_VIDEO_CALL";
            public const string AcceptVideoCall = "ACCEPT_VIDEO_CALL";
            public const string AcceptVideoCallResponse = "ACCEPT_VIDEO_CALL_RESPONSE";
            public const string RequestOpenDoor = "REQUEST_OPEN_DOOR";
            
            // ebkn
            public const string EBKNGetLastEventLogResponse = "GET_LAST_EVENT_LOG_RESPONSE";
            public const string TransmitUserData = "TRANSMIT_ALL_DATA";
            public const string AddUserEbkn = "ADD_USER_EBKN";
            public const string DeleteUserEbkn = "DELETE_USER_EBKN";
            public const string DeleteAllUserEbkn = "DELETE_ALL_USER_EBKN";

            // device request 
            public const string CheckFirmwareVersion = "CHECK_FIRMWARE_VERSION";
            public const string DeviceRequestSyncData = "SYNC_DATA";
            public const string NotificationNewFileLogDevice = "NOTIFICATION_NEW_FILE_LOG_DEVICE";

            public const string RefreshDeviceInfo = "REFRESH_DEVICE_INFO";
        }

        public class MessageType
        {
            public const string Info = "info";
            public const string Success = "success";
            public const string Failure = "failure";
            public const string Error = "error";
            public const string Warning = "warning";
        }

        public class ActionType
        {
            public const string Install = "Install";
            public const string Reinstall = "reinstall";
            public const string TransmitData = "transmit data";
            public const string SetTimeAndGetInfo = "refresh";
            public const string UpdateDevice = "update device";
        }

        public class LongProgressName
        {
            public const string Reinstall = "reinstalling";
            public const string TransmitData = "transmitting";
            public const string Downloading = "downloading";
            public const string Recorvering = "Recorvering";
            public const string Preparing = "Preparing";
            public const string Updating = "Updating";
            public const string Syncing = "Syncing";
            public const string ReinstallFailed = "failed";
            public const string TransmitDataFailed = "failed";
            public const string DownloadingFailed = "failed";
            public const string RecorveringFailed = "failed";
            public const string PreparingFailed = "failed";
            public const string UpdatingFailed = "failed";
        }
        public class LongProgressPercentage
        {
            public const decimal ReinstallStep0 = 5;
            public const decimal ReinstallStep1 = 30;
        }

        public class NotificationType
        {
            public const string InstallSuccess = "Install success";
            public const string InstallError = "Install error";

            public const string ReinstallSuccess = "Reinstall success";
            public const string ReinstallError = "Reinstall error";

            public const string TransmitDataSuccess = "Transmit data success";
            public const string TransmitDataError = "Transmit data error";

            public const string UploadSuccess = "Upload firmware success";
            public const string UploadError = "Upload firmware error";

            public const string SendDeviceInstructionSuccess = "Send device instruction success";
            public const string SendDeviceInstructionError = "Send device instruction error";
            public const string SendDeviceConfigSuccess = "Send device configration success";

            public const string FileTransferError = "Upload file error";
            public const string SendDeviceConfigError = "Send device config error";
            public const string SendUserError = "Send User error";
            public const string CheckCardFail = "Check card fail";

            public const string FaceError = "Face error";
            public const string NotificationCountReview = "NOTIFICATION_COUNT_REVIEW";

            public const string FireOn = "Fire On";
            public const string FireOff = "Fire Off";
        }

        public class CommandType
        {
            public const string Open = "Open";
            public const string SetTime = "SetTime";
            public const string Reset = "Reset";
            public const string ForceOpen = "ForceOpen";
            public const string ForceClose = "ForceClose";
            public const string Release = "Release";
            public const string DeleteAllUsers = "Delete_all_users";
            public const string DeleteAllEvents = "Delete_all_events";
            public const string UpdateDeviceState = "UpdateDeviceState";
            public const string UpdateFirmware = "UpdateFirmware";
            public const string StopUpdateFW = "Stop_Update_Firmware";
            public const string SendConfigLocalMqtt = "ConfigLocalMqtt";
            public const string ActiveFaceLicense = "ActiveFaceLicense";
            public const string TurnOnAlarm = "TURN_ON_ALARM";
            public const string TurnOffAlarm = "TURN_OFF_ALARM";
            // For Transmit all data (User)
            public const string UpdateAllUsers = "Update_all_users";
            public const string RequestLogFile = "RequestLogFile";
            public const string DownloadUsers = "DownloadUser";
            public const string RequestFirmwareVersion = "CHECK_FIRMWARE_VERSION";
        }

        public class ExportType
        {
            public const string LoadAllUser = "LoadAllUser";
            public const string LoadMasterCardUser = "LoadMasterCardUser";
            public const string LoadAllTimezone = "LoadAllTimezone";
        }

        public class AutoRenew
        {
            public const int SchedulerDay = 1;
            public const int SchedulerHour = 23;
            public const int SchedulerMinute = 59;
            public const int KeySize = 128;
            public const int BlockSize = 128;
        }

        public class DynamicQr
        {
            public const string AllowChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            public const string AllowCharsNumber = "0123456789";
            public const int LenPassCode = 6;
            public const string PassCodeString = "******";
            public const string Salt = "X";
            public const int MaxByte = 31;
            public const int LenghtOfSecretCode = 16;
            public const string NameProject = "DMPW";
            public const int MaxLengthQr = 80;
            public const int TimePeriod = 60;
            public const int SubStringSeconds = 2;
            public const string Key = "DualiDMPWDuali74";
        }

        public class Attendance
        {
            public const string ValidDays = "Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday";
            public const string ValidType = "Half,Full,Holiday";
            public const string DefaultName = "Standard Working Time";
            public const string DefaultWorkingTime = "[{\"Name\":\"Monday\",\"Start\":\"09:00\",\"End\":\"18:30\",\"Type\":\"Full\"},{\"Name\":\"Tuesday\",\"Start\":\"09:00\",\"End\":\"18:30\",\"Type\":\"Full\"},{\"Name\":\"Wednesday\",\"Start\":\"09:00\",\"End\":\"18:30\",\"Type\":\"Full\"},{\"Name\":\"Thursday\",\"Start\":\"09:00\",\"End\":\"18:30\",\"Type\":\"Full\"},{\"Name\":\"Friday\",\"Start\":\"09:00\",\"End\":\"18:30\",\"Type\":\"Full\"},{\"Name\":\"Saturday\",\"Start\":\"00:00\",\"End\":\"00:00\",\"Type\":\"Holiday\"},{\"Name\":\"Sunday\",\"Start\":\"00:00\",\"End\":\"00:00\",\"Type\":\"Holiday\"}]";
            public const int SchedulerAttendance = 4;
            public const int SchedulerNotifyCheckinLate = 1;
            public const int CheckAttendanceMinute = 15;
            public const string In = "In";
            public const string Out = "Out";
            public const int SchedulerDay = 1;
            public const int SchedulerHour = 23;
            public const int SchedulerMinute = 59;
            public const string RangTimeToDay = "Today";
            public const string RangTimeAllDay = "Allday";

            public const int DefaultWorkingHourType = 0;
        }

        public class PlugIn
        {
            public const string Common = "Common";
            public const string CardIssuing = "CardIssuing";
            public const string AccessControl = "AccessControl";
            public const string TimeAttendance = "TimeAttendance";
            public const string VisitManagement = "VisitManagement";
            public const string CanteenManagement = "CanteenManagement";
            public const string QrCode = "QrCode";
            public const string ScreenMessage = "ScreenMessage";
            public const string PassCode = "PassCode";
            public const string CameraPlugIn = "CameraPlugIn";
            public const string CameraPlugIn2 = "CameraPlugIn2";
            public const string EventManagement = "EventManagement";
            public const string VehiclePlugIn = "VehiclePlugIn";
            public const string Covid19 = "Covid19";
            // [Edward] 2020.04.22
            // Make plugin about military
            public const string FingerPrint = "FingerPrint";
            public const string DepartmentAccessLevel = "DepartmentAccessLevel";
            public const string CustomizeLanguage = "CustomizeLanguage";
        }

        public class PlugInValue
        {
            public const bool CardIssuing = false;
            public const bool AccessControl = true;
            public const bool TimeAttendance = false;
            public const bool VisitManagement = false;
            public const bool CanteenManagement = false;
            public const bool Common = true;
            public const bool ScreenMessage = false;
            public const bool QrCode = false;
            public const bool PassCode = false;
            public const bool CameraPlugIn = false;
            public const bool ArmyManagement = false;


            public const string CardIssuingDescription = "";
            public const string AccessControlDescription = "This function enable for the company that need to control door. This including door lock management ...";
            public const string TimeAttendanceDescription = "Function related to Time Attendance management. E.g Clockin / clock out report, working time, holiday application";
            public const string VisitManagementDescription = "Provice function for managing visitors and visit such as sending invitation, access level, visit report ...";
            public const string CanteenManagementDescription = "";
            public const string CommonDescription = "The function that apply for all the system";
            public const string ScreenMessageDescription = "This plugins enable for the company that use readers that have screen. E.g iTouchPop 2A";
            public const string QrCodeDescription = "This plugins enable for the company that use QR code as identification";
            public const string PassCodeDescription = "This plugins enable for the company that use Pass Code as identification. Use supported device such as: DE960, iTouchPop 2A";
            public const string CameraPlugInDescription = "";
            
            public const string ArmyManagementDescription = "";
        }

        public class Link
        {
            public const string Android_Download = "https://play.google.com/store/apps/details?id=com.demasterpro";
            public const string IOS_Download = "https://apps.apple.com/us/app/id1500403553";
        }

        public class Condition
        {
            public const bool AND = true;
            public const bool OR = false;
        }

        public class Dashboard
        {
            /// <summary>
            /// Number of hour point dislay in dashboards
            /// </summary>
            public const int LastHourNumber = 12;
        }

        public enum VariableEmailTemplate
        {
            CompanyName,
            VisitorName,
            ApprovalName,
            ApprovalCount,
            VisitTargetName,
            VisitLink,
            Address,
            VisitorAvatar,
            ResultCheckin,
            VisitorPosition,
            VisitorPhone,
            VisitReason,
            VisitUserCreate,
            VisitUserReject,
            VisitTimeAccess,
            VisitEmail,
            VisitLinkQrCode,
            LogoImageId,
            ImageQRCode,
            AndroidIcon,
            IosIcon,
            UserNameWelcome,
            EmailWelcome,
            Token,
            UserName,
            PassCode,
            PasswordDefault,
            FireCrackerIcon,
            GetItOnGooglePlayIcon,
            DownloadOnAppStoreIcon,
            CompanyLogo,
            TimeIcon,
            LocationIcon,
            DoorIcon,
            DoorListVisitAccess,
            PhoneIcon,
            ContactVisitTarget,
            UserNameBorrowBook,
            BookNameBorrowBook,
            BorrowDateBorrowBook,
            DeadlineDateBorrowBook,
            LeaveUserName,
            LeaveType,
            LeaveFromDate,
            LeaveToDate,
            LeaveReason,
            LeaveRejectReason,
            LeaveRequestLink,
            ManagerApproval,

            LogoSquare,
            TextInvite,
            ImgVisit1,
            ImgVisit2,
            NoteScanQR,
            TextPowerBy,
            
            QRCodeLink,
            Background
        }

        public enum TypeEmailTemplate
        {
            StartZero = 0, // index = 0, ignore, not use
            RequestApproval = 1,
            ApprovalVisit = 2,
            RejectVisit = 3,
            LinkRegisterVisit = 4,
            WelcomeUserEmail = 5,
            SendPassCodeEmail = 6,
            SendQREmail = 7,
            WelcomeMailForManager = 8,
            WelcomeMailForAdmin = 9,
            NotifyOverDueBook = 10,
            LeaveRequestReview = 11,
            LeaveRequestApproval = 12,
            LeaveRequestReject = 13,
            QRCardTemplate = 14,
            MonitoringTemplate = 15,
        }
        
        // link api camera hanet
        public class HanetApiCamera
        {
            public const string ApiRefreshToken = "https://oauth.hanet.com/token?grant_type=refresh_token&client_id={0}&client_secret={1}&refresh_token={2}";
            public const string HostServer = "https://partner.hanet.ai";
            
            public const string GetListPlaces = "/place/getPlaces";
            public const string GetTotalPersonByPlaceID = "/person/getTotalPersonByPlaceID";
            public const string PersonGetListByPlace = "/person/getListByPlace";
            public const string PersonRegister = "/person/register";
            // public const string PersonRemoveByPlace = "/person/removeByPlace";
            public const string PersonRemoveByID = "/person/removePersonByID";
            public const string PersonRemoveAllByPlace = "/person/removeAllPersonInPlace";
            public const string PersonUpdateAvatar = "/person/updateByFaceImage";
            public const string PersonUpdateAliasId = "/person/updateAliasID";
            public const string GetCheckinByPlaceInTimeSpan = "/person/getCheckinByPlaceIdInTimestamp";
            public const string GetConnectionStatus = "/device/getConnectionStatus";
            public const string GetListDeviceByPlace = "/device/getListDeviceByPlace";
            public const string PersonTakeFacePicture = "/person/takeFacePicture";
            public const string TakeFramePicture = "/api/takeFramePictureByToken";
            public const string GetListDepartment = "/department/list";
            public const string GetListPersonByDepartmentId = "/department/list-person";

            public const int DefaultDelayCallApi = 10; // 10s
            public const string TopicNotification = ".topic.camera.{0}.{1}"; // .topic.camera.{companyCode}.{cameraId}
            public const string TopicProcessUpdate = ".topic.camera.{0}"; // .topic.camera.{companyCode}
            public const string PrefixAliasIdVisitor = "visitor.";
            public const string PrefixCardId = "H";
            public const int TimeoutWebhook = 4; // 4s
            public const int TimeCronJobUpdateImageEventLog = 60; // 1s
            public const int TimeoutWebhookTakeFacePicture = 30; // 30s
            public const int TimeoutWaitingTakeAPicture = 30; // 30s
            public const int MaximumPageSizeOfRequest = 50;

            public const string TypeCameraLoadInfo = "HANET_CAMERA";
            public const string DefineConfigCertFile = "CameraHanet:CertFile";
            public const string DefineConfigIgnoredSsl = "CameraHanet:IgnoredSSL";
            public const string StatusCodeTokenExpired = "401";
            public const int CountTryRefreshToken = 2;
        }

        public class SettingPrinter
        {
            public const string TypePrinterStatus = "PRINTER_STATUS";
            public const string TypePrinterCard = "PRINTER_CARD";
            public const string TypePrinterReadData = "PRINTER_READ_DATA";
            public const string TypePrinterWriteData = "PRINTER_WRITE_DATA";
            
            public static byte[] DefaultReadBlock0 = {0x00, 0x02, 0x27, 0x00, 0x25};
            public const int MaxSizeImage = 1000;
            public const int MaxWidthCard = 372;
            public const int MaxHeightCard = 236;
            
            // Smart Card Printer
            public const int WidthDefaultSmartCard = 636;
            public const int HeightDefaultSmartCard = 1012;
            public const int HeightDefaultBackSmartCardIC = 652;
            public const string FolderCardPrinter = "card_printer";
            public const string FileTemplateTelnet = "telnet_smart_card_printer.c";
            public const string FileTemplateTelnetOut = "telnet_smart_card_printer.out";
            public const string DataImageFontLayout = "[DATA_IMAGE_FONT_LAYOUT]";
            public const string DataImageBackLayout = "[DATA_IMAGE_BACK_LAYOUT]";
            public const string CodePrintCard = "[OVERRIDE_CODE_PRINT_CARD]";
            // public const string CommandCodePrintStampFont = "[COMMAND_CODE_PRINT_STAMP_FONT]";
            public const string CommandCodePrintStampBack = "[COMMAND_CODE_PRINT_STAMP_BACK]";
            
            // init field table card layout
            public static readonly string[] UserColumns = {"Address", "UserCode", "City", "ExpiredDate", "EffectiveDate", "EmpNumber", "Name", "Email", "Sex", "BirthDay", "Job", "HomePhone", "Nationality", "OfficePhone", 
                "Position", "PostCode", "MilitaryNumber", "ArmyUserFamily Name", "ArmyUserFamily Rank", "ArmyUserFamily Department"};
            public static readonly string[] DepartmentColumns = {"DepartName", "DepartNo"};
            public static readonly string[] CardColumns = {"CardId", "ManagementNumber"};
            public static readonly string[] VehicleColumns = { "PlateNumber", "Model", "PlateRFID", "Vehicle Type", "Vehicle Classification" };
            
            // CX-D80 config
            public const int CXD80MaxWidthCard = 1036;
            public const int CXD80MaxHeightCard = 664;
        }
        
        public class VerifyLicenseSetting
        {
            public const bool IgnoredCheckFromLicense = true; // if using system have license IgnoredCheckFromLicense = false
            public const string DefineHostVerifyLicense = "VerifyLicenseSetting:Host";
            public const int KeySize = 4096;
            public const string FileNameDemasterProKey = "DeMasterProKey.pem";
            public const string TextBeginDemasterProKey = "-----BEGIN DEMASTERPRO KEY-----";
            public const string TextEndDemasterProKey = "-----END DEMASTERPRO KEY-----";

            public const string ApiLicenseKeyActive = "/license-key/active";
        }

        public class ExportFileSetting
        {
            public const int MaximumRecordExport = 100000;
            public const int MaximumRecordHancellExport = 20000;

            public const int AllowMaximumRecordExport = 1000000;
            public const int AllowMaximumRecordHancellExport = 300000;
        }
        
        public class LFaceConfig
        {
            public const string PrefixVisitId = "V";
        }

        public class NotificationTypeMqtt
        {
            public const string ExportCardQR = "EXPORT_CARD_QR";
            public const string CallToUser = "CALL_TO_USER";
        }
        public class MessageTypeMqtt
        {
            public const string Info = "info";
            public const string Error = "error";
        }
        public class InvitationLinkSender
        {
            public const string MeetingSender = "Meeting_Visit_Invitation_Link";
        }
        public class ImageConfig
        {
            public const string BaseFolderMeeting = "meeting";
            public const string PrefixVisitCheckinFile = "register_visit";
        }

        public class VmsConfig
        {
            public const string DefineHost = "VmsConfig:Host";
            public const string DefineSecretKey = "VmsConfig:SecretKey";
            public const string ApiCameras = "/vms-api/cameras";
            public const string ApiCamerasGetVideo = "/vms-api/cameras/{cameraId}/video";
        }

        public class SDKDevice
        {
            public const string DefineConfig = "SDKDeviceConfig";
            public const string SenderDefault = "DMP_API";
            public const int MaxPageSize = 100;
            public const string PrefixLFacePass = "M_";
            public const string PrefixLFaceAratek = "A_";

            public const string WebhookDeviceConnectionType = "DEVICE_CONNECTION";
            public const string WebhookEventLogType = "EVENT_LOG";
            public const string WebhookDoorStatusType = "DOOR_STATUS";
            public const string WebhookDeviceRequestType = "DEVICE_REQUEST";
            public const string WebhookProcessData = "PROCESS_DATA";
            public const string WebhookNotification = "NOTIFICATION";

            public const string ApiLogin = "/login";
            public const string ApiRefreshToken = "/refreshToken";
            public const string ApiDeviceInSubnet = "/device/get-device-in-subnet";
            public const string ApiGetDeviceInfo = "/device/get-device-info";
            public const string ApiSetCurrentTime = "/device/set-current-time";
            public const string ApiUpdateTimeZone = "/device/update-timezone";
            public const string ApiUpdateHoliday = "/device/update-holiday";
            public const string ApiDeviceInstruction = "/device/device-instruction";
            public const string ApiOpenDoor = "/device/open-door";
            public const string ApiAddCard = "/device/add-card";
            public const string ApiDeleteCard = "/device/delete-card";
            public const string ApiUpdateDeviceConfig = "/device/update-device-config";
            public const string ApiDeviceReceiveEventLog = "/device/event-log";
            public const string ApiDeviceLoadTimeZone = "/device/load-timezone";
            public const string ApiDeviceLoadUserInfo = "/device/get-card";
            public const string ApiUpdateFirmware = "/device/update-file-firmware";
            
            public const string ApiDeviceRequest = "/device/device-request";
        }
    }
}
