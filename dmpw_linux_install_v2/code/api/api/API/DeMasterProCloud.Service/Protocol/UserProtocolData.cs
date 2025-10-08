using System;
using System.Collections.Generic;
using DeMasterProCloud.DataAccess.Models;

namespace DeMasterProCloud.Service.Protocol
{
    /// <summary>
    /// Sending user data to Icu
    /// </summary>
    public class UserProtocolData : ProtocolData<UserProtocolHeaderData>
    {
        public UserProtocolData()
        {
        }
    }

    public class UserProtocolHeaderData
    {
        public UserProtocolHeaderData()
        {
            Users = new List<UserProtocolDetailData>();
        }
        public int Total { get; set; }

        //김창환 - User 프로토콜 변경 오후 4:21 2019-09-16
        //UpdateFlag 속성 추가 - // 2019.09.05 유저 전송의 끝을 알 수 있는 플래그. 0: 뒤의 데이터가 더 있음, 1: 마지막 데이터(권승재 주임)
        public int UpdateFlag { get; set; }

        public int FrameIndex { get; set; }
        public int TotalIndex { get; set; }

        public List<UserProtocolDetailData> Users { get; set; }
    }

    public class UserProtocolDetailData
    {
        public string EmployeeNumber { get; set; }
        public string UserName { get; set; }
        public string DepartmentName { get; set; }
        public string CardId { get; set; }
        public int IssueCount { get; set; }
        public short AdminFlag { get; set; }
        public string EffectiveDate { get; set; }
        // public string EffectiveDateTime { get; set; }
        public string ExpireDate { get; set; }
        // public string ExpireDateTime { get; set; }
        public int CardStatus { get; set; }
        public int AntiPassBack { get; set; }
        public int Timezone { get; set; }
        public string Password { get; set; }
        public int AccessGroupId { get; set; }
        public string UserId { get; set; }
        public int IdType { get; set; }

        public string Position { get; set; }

        public string Avatar { get; set; }


        // For Face ID
        public FaceDataList FaceData { get; set; }
        public List<int> FloorIndex { get; set; }
        public string WorkType { get; set; }
        public string Grade { get; set; }
        
        // For finger print
        public List<string> FingerTemplates { get; set; }
    }

    //public class ArmyUserProtocolDetailData : UserProtocolDetailData
    //{
    //    public string MilitaryNumber { get; set; }
    //    public string WorkType { get; set; }
    //    public string Grade { get; set; }
    //}

    //public class UserAvatarProtocolDetailData : UserProtocolDetailData
    //{
    //    // User's avatar image encoded in base64.
    //    public string Avatar { get; set; }
    //}

    /// <summary>
    /// User log protocol data
    /// </summary>
    public class UserLogProtocolData
    {
        public UserLogProtocolData()
        {
            UserLogs = new List<UserLog>();
        }
        public string IcuAddress { get; set; }
        public List<UserLog> UserLogs { get; set; }
        public string ProtocolType { get; set; }
    }

    public class UserLog
    {
        public long Id { get; set; }
        public short Action { get; set; }
        public string CardId { get; set; }
        public int CardType { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public int IcuId { get; set; }
        public string KeyPadPw { get; set; }
        public int TzPosition { get; set; }
        public int UserId { get; set; }
        public short TransferStatus { get; set; }
        public IcuDevice IcuDevice { get; set; }
        public User User { get; set; }
        public Visit Visit { get; set; }
        public string DepartmentName { get; set; }
        public int IssueCount { get; set; }

        public int CardStatus { get; set; }
        public int AntiPassBack { get; set; }
        public List<int> FloorIndex { get; set; }

        // IsMasterCard
        public bool MasterFlag { get; set; }

        public string Position { get; set; }
        public string Avatar { get; set; }


        // For army
        public string MilitaryNumber { get; set; }
        public string WorkType { get; set; }
        public string Grade { get; set; }
        
        // For finger print
        public List<string> FingerTemplates { get; set; }


        // CompanyId
        public int CompanyId { get; set; }
    }

    //public class UserAvatarLog : UserLog
    //{
    //    public string Avatar { get; set; }
    //}

    /// <summary>
    /// Add user response protocol data
    /// </summary>
    public class UserResponseProtocolData : ProtocolData<UserResponseHeaderData>
    {
        public int CountUser { get; set; }
        public int CountEvent { get; set; }
    }

    public class UserResponseHeaderData
    {
        public UserResponseHeaderData()
        {
            Users = new List<UserResponseDetailData>();
        }
        public int Total { get; set; }
        public List<UserResponseDetailData> Users { get; set; }
    }

    public class UserResponseDetailData
    {
        public string CardId { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// Load user protocol data
    /// </summary>
    public class LoadUserProtocolData : ProtocolData<LoadUserProtocolHeader>
    {
    }
    public class LoadUserProtocolHeader { }


    /// <summary>
    /// Load user by cardId protocol data
    /// </summary>
    public class LoadUserByCardIdProtocolData : ProtocolData<LoadUserByCardIdProtocolHeader>
    {
    }

    public class LoadUserByCardIdProtocolHeader
    {
        public string CardId { get; set; }
    }

    /// <summary>
    /// Load user response
    /// </summary>
    public class LoadUserResponseProtocolData : ProtocolData<LoadUserResponseHeader>
    {

    }

    public class LoadUserResponseHeader
    {
        public int FrameIndex { get; set; }
        public int TotalIndex { get; set; }
        public int Total { get; set; }
        public List<UserDetail> Users { get; set; }
    }

    public class UserDetail
    {
        public string EmployeeNumber { get; set; }
        public string DepartmentName { get; set; }
        public string UserName { get; set; }
        public string CardId { get; set; }
        public string ExpireDate { get; set; }
        public string EffectiveDate { get; set; }
        public int IssueCount { get; set; }
        public int AdminFlag { get; set; }
        public int CardStatus { get; set; }
        public string Password { get; set; }
        public int Timezone { get; set; }

        // Some devices send 'departmentCode' rather than 'departmentName'
        public string DepartmentCode { get; set; }
        // Some devices send 'TimezonePos' rather than 'timezone'
        public int TimezonePos { get; set; }
    }

    /// <summary>
    /// Send user protocol data
    /// </summary>
    public class SendUserProtocolData : ProtocolData<SendUserResponseHeader>
    {
    }

    public class SendUserResponseHeader
    {
        public int FrameIndex { get; set; }
        public int TotalIndex { get; set; }
        public int IcuId { get; set; }
        public string DeviceType { get; set; }
        public string Target { get; set; }
        public int Total { get; set; }
        public List<SendUserDetail> Users { get; set; }
    }

    public class SendUserDetail
    {
        public int Id { get; set; }
        public string CardId { get; set; }
        public string UserName { get; set; }
        public string Department { get; set; }
        public string EmployeeNumber { get; set; }
        public int IssueCount { get; set; }
        public int IsMasterCard { get; set; }
        public string ExpireDate { get; set; }
        public string EffectiveDate { get; set; }
        public string CardStatus { get; set; }
        public string Password { get; set; }
        public string Timezone { get; set; }
    }

    public class UserInfoModel
    {
        public SendUserDetail UserDb { get; set; }
        public SendUserDetail UserDevice { get; set; }
    }

    public class UserInfoByCardIdModel
    {
        public string MsgId { get; set; }
        public SendUserDetail User { get; set; }
    }


    public class RequestUserProtocolData : ProtocolData<RequestUserDetailData>
    {
    }

    public class RequestUserDetailData
    {
        public string DeviceAddress { get; set; }
        public string LastDate { get; set; }
    }


    public class RequestUserResponseData : ProtocolData<RequestUserResponseDetailData>
    {
    }

    public class RequestUserResponseDetailData
    {
        public string LastDate { get; set; }
    }
}
