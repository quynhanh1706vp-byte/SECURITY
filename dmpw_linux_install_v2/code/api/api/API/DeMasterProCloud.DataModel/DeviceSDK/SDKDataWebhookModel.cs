using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.DeviceSDK;

public class SDKDataWebhookModel
{
    public string Type { get; set; }
    public object Data { get; set; }
}

public class SDKDeviceConnectionModel
{
    public string DeviceAddress { get; set; }
    public int Status { get; set; }
    public string IpAddress { get; set; }
    public string DeviceType { get; set; }
    public string MacAddress { get; set; }
    public string OperationMode { get; set; }
    public string ClientId { get; set; }
}
public class SDKEventLogHeaderData
{
    public SDKEventLogHeaderData()
    {
        Events = new List<SDKEventLogModel>();
    }
    public int Total { get; set; }
    public List<SDKEventLogModel> Events { get; set; }

    public int UtcHour { get; set; }
    public int UtcMinute { get; set; }
}
public class SDKEventLogModel
{
    public string DeviceAddress { get; set; }
    public string AccessTime { get; set; }
    public string CardId { get; set; }
    public int IssueCount { get; set; }
    public string UserId { get; set; }
    public string UpdateTime { get; set; }
    public string InOut { get; set; }
    public int EventType { get; set; }
    public int IdType { get; set; }
    public double Temperature { get; set; }
    public double Distance { get; set; }
    public double SearchScore { get; set; }
    public double LivenessScore { get; set; }
    public string OtherCardId { get; set; }
}
public class SDKDoorStatusModel
{
    public string DeviceAddress { get; set; }
    // 0 : Closed & Lock
    // 1 : Closed & Unlocked
    // 2 : Opened
    // 3 : Fored Opened
    // 4 : Passage Opened
    // 5 : Emergency Opened
    // 6 : Emergency Closed
    public int DoorState { get; set; }
}
public class SDKDeviceRequestModel
{
    public string DeviceAddress { get; set; }
    public string RequestType { get; set; }
}
public class SDKEventLogFe
{
    public Guid Id { get; set; }
    public object EventLogId { get; set; }
    public int DeviceId { get; set; }
    public string DeviceName { get; set; }
    public string DeviceAddress { get; set; }
    public int BuildingId { get; set; }
    public string BuildingName { get; set; }
    public int? UserId { get; set; }
    public int? VisitId { get; set; }
    public string AccessTime { get; set; }
    public double UnixTime { get; set; }
    public string EventTypeDescription { get; set; }
    public int EventType { get; set; }
    public int CardStatus { get; set; }
    public string CardId { get; set; }
    public string CardType { get; set; }
    public int CardTypeId { get; set; }
    public int IssueCount { get; set; }
    public string UserName { get; set; }
    public string Avatar { get; set; }
    public string ResultCheckIn { get; set; }
    public string InOut { get; set; }
    public double BodyTemperature { get; set; }
    public string DepartmentName { get; set; }
    public string Position { get; set; }
    public bool IsVisit { get; set; }
    public int WorkType { get; set; }
    public string WorkTypeName { get; set; }
    public string ImageCamera { get; set; }
    public double Distance { get; set; }
    public double SearchScore { get; set; }
    public double LivenessScore { get; set; }
    public string NationalIdNumber { get; set; }
    public string Videos { get; set; }
    public string OtherCardId { get; set; }
}

public class SDKProcessFeModel
{
    public string MsgId { get; set; }
    public int Index { get; set; }
    public int Total { get; set; }
}