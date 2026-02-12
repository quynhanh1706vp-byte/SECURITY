using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMasterProCloud.Common.Infrastructure.Header
{
    /// <summary>
    /// All headers related with user are in this enum list.
    /// </summary>
    public enum UserHeaderColumns
    {
        [Localization(nameof(UserResource.lblId), typeof(UserResource))]
        Id = 0,
        [Localization(nameof(UserResource.lblName), typeof(UserResource))]
        FirstName,
        [Localization(nameof(UserResource.lblUserCode), typeof(UserResource))]
        UserCode,
        [Localization(nameof(UserResource.lblDepartment), typeof(UserResource))]
        DepartmentName,
        [Localization(nameof(UserResource.lblEmployeeNumber), typeof(UserResource))]
        EmployeeNo,
        [Localization(nameof(UserResource.lblMilitaryNumber), typeof(UserResource))]
        MilitaryNo,
        [Localization(nameof(UserResource.lblExpiredDate), typeof(UserResource))]
        ExpiredDate,
        [Localization(nameof(UserResource.lblCardId), typeof(UserResource))]
        CardList,
        [Localization(nameof(UserResource.lblAction), typeof(UserResource))]
        Action,
        [Localization(nameof(UserResource.lblApprovalStatus), typeof(UserResource))]
        ApprovalStatus,
        [Localization(nameof(EventLogResource.lblPlateNumber), typeof(EventLogResource))]
        PlateNumberList,
        [Localization(nameof(UserResource.lblPosition), typeof(UserResource))]
        Position,
        [Localization(nameof(UserResource.lblWorkType), typeof(UserResource))]
        WorkTypeName,
        [Localization(nameof(UserResource.lblAccessGroup), typeof(UserResource))]
        AccessGroupName,
        [Localization(nameof(AccountResource.lblAccount), typeof(AccountResource))]
        Email,
        [Localization(nameof(EventLogResource.lblFace), typeof(EventLogResource))]
        FaceList,
    }


    public enum EventsHeaderColumns
    {
        [Localization(nameof(UserResource.lblId), typeof(UserResource))]
        Id = 0,
        [Localization(nameof(EventLogResource.lblAccessTime), typeof(EventLogResource))]
        EventTime,
        [Localization(nameof(EventLogResource.lblUserName), typeof(EventLogResource))]
        UserName,
        [Localization(nameof(UserResource.lblDepartment), typeof(UserResource))]
        DepartmentName,
        [Localization(nameof(UserResource.lblCardId), typeof(UserResource))]
        CardId,
        [Localization(nameof(UserResource.lblCardStatus), typeof(UserResource))]
        CardStatus,
        [Localization(nameof(UserResource.lblCardType), typeof(UserResource))]
        CardType,
        [Localization(nameof(EventLogResource.lblDoorName), typeof(EventLogResource))]
        DoorName,
        [Localization(nameof(DeviceResource.lblDeviceId), typeof(DeviceResource))]
        DeviceAddress,
        [Localization(nameof(EventLogResource.lblSite2), typeof(EventLogResource))]
        Building,
        [Localization(nameof(EventLogResource.lblInOut), typeof(EventLogResource))]
        InOut,
        [Localization(nameof(EventLogResource.lblEventType), typeof(EventLogResource))]
        EventType,
        [Localization(nameof(EventLogResource.lblEventDetail), typeof(EventLogResource))]
        EventDetail,
        [Localization(nameof(EventLogResource.lblIssueCount), typeof(EventLogResource))]
        IssueCount,
        [Localization(nameof(UserResource.lblMilitaryNumber), typeof(UserResource))]
        MilitaryNo,
        [Localization(nameof(UserResource.lblPosition), typeof(UserResource))]
        Position,
        [Localization(nameof(UserResource.lblWorkType), typeof(UserResource))]
        WorkTypeName,
        [Localization(nameof(UserResource.lblExpiredDate), typeof(UserResource))]
        ExpireDate,
        [Localization(nameof(EventLogResource.lblEventMemo), typeof(EventLogResource))]
        Memo,
        [Localization(nameof(EventLogResource.lblVehicleImage), typeof(EventLogResource))]
        VehicleImage,

        // For vehicle
        [Localization(nameof(EventLogResource.lblVehicleModel), typeof(EventLogResource))]
        Model,
        [Localization(nameof(EventLogResource.lblPlateNumber), typeof(EventLogResource))]
        PlateNumber,
        [Localization(nameof(EventLogResource.lblVehicleType), typeof(EventLogResource))]
        VehicleType,

        [Localization(nameof(AccountResource.lblContact), typeof(AccountResource))]
        Contact,

        [Localization(nameof(MailContentResource.BodyVisitPurpose), typeof(MailContentResource))]
        VisitPurpose,

        [Localization(nameof(UserResource.lblRemarks), typeof(UserResource))]
        Remark,

        [Localization(nameof(EventLogResource.lblRemark1), typeof(EventLogResource))]
        Remark1,
        [Localization(nameof(EventLogResource.lblRemark2), typeof(EventLogResource))]
        Remark2,
    }

    /// <summary>
    /// All headers related with Device are in this enum list.
    /// </summary>
    public enum DeviceHeaderColumns
    {
        [Localization(nameof(EventLogResource.lblIDX), typeof(EventLogResource))]
        Id = 0,
        [Localization(nameof(DeviceResource.lblDoorName), typeof(DeviceResource))]
        DoorName,
        [Localization(nameof(EventLogResource.lblDoorName), typeof(EventLogResource))]
        DoorName_Army,
        [Localization(nameof(DeviceResource.lblDeviceId), typeof(DeviceResource))]
        DeviceAddress,
        [Localization(nameof(DeviceResource.lblVerifyMode), typeof(DeviceResource))]
        VerifyMode,
        [Localization(nameof(DeviceResource.lblBuilding), typeof(DeviceResource))]
        Building,
        [Localization(nameof(DeviceResource.lblConnectionStatus), typeof(DeviceResource))]
        ConnectionStatus,
        [Localization(nameof(DeviceResource.lblVersion), typeof(DeviceResource))]
        Version,
        [Localization(nameof(DeviceResource.lblLastCommunicationTime), typeof(DeviceResource))]
        LastCommunicationTime,
        [Localization(nameof(DeviceResource.lblDeviceType), typeof(DeviceResource))]
        DeviceType,
        [Localization(nameof(DeviceResource.lblNumberOfNotTransmittingEvent), typeof(DeviceResource))]
        NumberOfNotTransmittingEvent,
        [Localization(nameof(DeviceResource.lblRegisterIdNumber), typeof(DeviceResource))]
        RegisterIdNumber,
        [Localization(nameof(DeviceResource.lblDoorStatus), typeof(DeviceResource))]
        DoorStatus,
        [Localization(nameof(UserResource.lblAction), typeof(UserResource))]
        Action,

        [Localization(nameof(DeviceResource.lblProgress), typeof(DeviceResource))]
        Progress,

        [Localization(nameof(DeviceResource.lblDoorActiveTimezone), typeof(DeviceResource))]
        ActiveTz,

        [Localization(nameof(DeviceResource.lblDoorPassageTimezone), typeof(DeviceResource))]
        PassageTz,

        [Localization(nameof(DeviceResource.lblAccessTimezone), typeof(DeviceResource))]
        AccessTime,

        [Localization(nameof(DeviceResource.lblAntiPassback), typeof(DeviceResource))]
        AntipassBack,
        [Localization(nameof(DeviceResource.lblMpr), typeof(DeviceResource))]
        Mpr,
    }


    public enum SystemLogHeaderColumns
    {
        [Localization(nameof(UserResource.lblId), typeof(UserResource))]
        Id = 0,
        [Localization(nameof(SystemLogResource.lblOperationTime), typeof(SystemLogResource))]
        OperationTime,
        [Localization(nameof(SystemLogResource.lblUserAccount), typeof(SystemLogResource))]
        UserAccount,
        [Localization(nameof(SystemLogResource.lblOperationType), typeof(SystemLogResource))]
        OperationType,
        [Localization(nameof(SystemLogResource.lblAction), typeof(SystemLogResource))]
        OperationAction,
        [Localization(nameof(SystemLogResource.lblMessage), typeof(SystemLogResource))]
        Message,
        [Localization(nameof(SystemLogResource.lblMessageDetail), typeof(SystemLogResource))]
        Details
    }
}
