using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Meeting
{
    public class MeetingModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int TimeoutAllowed { get; set; }
        public int MeetingRoomId { get; set; }
        public bool UseAlarm { get; set; }
        public int LimitAttendance { get; set; }
        public List<int> UserIds { get; set; }
        public List<int> VisitIds { get; set; }
        public InvitationLinkTemplate InvitationLinkTemplate { get; set; }
    }
    public class MeetingDetailModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int TimeoutAllowed { get; set; }
        
        public int MeetingRoomId { get; set; }
        public string MeetingRoomName { get; set; }
        public bool UseAlarm { get; set; }
        public int LimitAttendance { get; set; }
        public List<UserMeetingModel> Users { get; set; }
        public List<VisitMeetingModel> Visits { get; set; }
        public InvitationLinkTemplate InvitationLinkTemplate { get; set; }
    }

    public class InvitationLinkTemplate
    {
        public string ListFieldsEnable { get; set; }
        public List<string> FieldRequired { get; set; }
        public List<string> FieldRegisterLeft { get; set; }
        public List<string> FieldRegisterRight { get; set; }
        public string LabelListField { get; set; }
        public string BodyTemplate { get; set; }
    }
    public class UserMeetingModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string DepartmentName { get; set; }
    }
    public class VisitMeetingModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
    }
    
    public class MeetingListModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int MeetingRoomId { get; set; }
        public string MeetingRoomName { get; set; }
        public int TimeoutAllowed { get; set; }
        public bool UseAlarm { get; set; }
        public int LimitAttendance { get; set; }
        public bool StatusInstruction { get; set; }
    }

    public class MeetingDeviceInstruction
    {
        public List<string> MsgIds { get; set; }
        public int DeviceId { get; set; }
        public bool StatusInstruction { get; set; }
    }

    public class MeetingUserInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string DepartmentName { get; set; }
    }
}