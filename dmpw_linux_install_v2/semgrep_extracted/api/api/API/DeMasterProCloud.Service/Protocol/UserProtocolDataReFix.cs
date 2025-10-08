using System;
using System.Collections.Generic;
using DeMasterProCloud.DataAccess.Models;

namespace DeMasterProCloud.Service.Protocol
{
    #region Basic info

    public class CardInfoBasic : ICloneable
    {
        public int Id { get; set; }
        public DateTime? EffectiveDateUtc { get; set; }
        public DateTime? ExpireDateUtc { get; set; }
        public string EffectiveDate { get; set; }
        public string ExpireDate { get; set; }
        public string EmployeeNumber { get; set; }
        public string UserName { get; set; }
        public string DepartmentName { get; set; }
        public string CardId { get; set; }
        public int IssueCount { get; set; }
        public short AdminFlag { get; set; }
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
        
        // Elevator
        public List<int> FloorIndex { get; set; }
        
        // IsMasterCard
        public bool IsMasterCard { get; set; }
        public bool MasterFlag { get; set; }
        
        // For finger print
        public List<string> FingerTemplates { get; set; }
        
        // All data user and visitor
        public User User { get; set; }
        public Visit Visit { get; set; }

        public object Clone()
        {
            var clone = (CardInfoBasic)MemberwiseClone();
            return clone;
        }
    }

    #endregion
    
    #region UserProtocolDataIcu

    public class UserProtocolDataIcu : ProtocolData<UserProtocolIcuHeaderData>
    {
    }

    public class UserProtocolIcuHeaderData
    {
        public UserProtocolIcuHeaderData()
        {
            Users = new List<UserProtocolDetailDataIcu>();
        }
        public int Total { get; set; }

        public int UpdateFlag { get; set; }

        public int FrameIndex { get; set; }
        public int TotalIndex { get; set; }

        public List<UserProtocolDetailDataIcu> Users { get; set; }
    }

    public class UserProtocolDetailDataIcu
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string EmployeeNumber { get; set; }
        public string CardId { get; set; }
        public int IssueCount { get; set; }
        public short AdminFlag { get; set; }
        public string EffectiveDate { get; set; }
        public string ExpireDate { get; set; }
        public int CardStatus { get; set; }
        public int AntiPassBack { get; set; }
        public int Timezone { get; set; }
        public string Password { get; set; }
        public int IdType { get; set; }
    }

    #endregion
    
    #region UserProtocolDataIcuElevator

    public class UserProtocolDataIcuElevator : ProtocolData<UserProtocolIcuElevatorHeaderData>
    {
    }

    public class UserProtocolIcuElevatorHeaderData
    {
        public UserProtocolIcuElevatorHeaderData()
        {
            Users = new List<UserProtocolDetailDataIcuElevator>();
        }
        public int Total { get; set; }

        public int UpdateFlag { get; set; }

        public int FrameIndex { get; set; }
        public int TotalIndex { get; set; }

        public List<UserProtocolDetailDataIcuElevator> Users { get; set; }
    }

    public class UserProtocolDetailDataIcuElevator
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string EmployeeNumber { get; set; }
        public string CardId { get; set; }
        public int IssueCount { get; set; }
        public short AdminFlag { get; set; }
        public string EffectiveDate { get; set; }
        public string ExpireDate { get; set; }
        public int CardStatus { get; set; }
        public int AntiPassBack { get; set; }
        public int Timezone { get; set; }
        public string Password { get; set; }
        public int IdType { get; set; }
        public List<int> FloorIndex { get; set; }
    }

    #endregion
    
    #region UserProtocolDataCommon

    public class UserProtocolDataCommon : ProtocolData<UserProtocolHeaderData>
    {
        public List<string> Receivers { get; set; }
    }

    #endregion
}