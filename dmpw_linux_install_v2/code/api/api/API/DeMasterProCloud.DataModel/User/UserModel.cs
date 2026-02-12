using System.Collections.Generic;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataModel.AccessGroup;
using DeMasterProCloud.DataModel.Department;
using DeMasterProCloud.DataModel.Category;
using Newtonsoft.Json;
using System;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Card;
using DeMasterProCloud.DataModel.EventLog;

namespace DeMasterProCloud.DataModel.User
{
    public enum Sex
    {
        Male = 1,
        Female = 2
    }

    public enum FileType
    {
        Excel = 0,
        Txt = 1
    }

    public class UserModel
    {
        //[JsonIgnore]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string BirthDay { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentNo { get; set; }
        public string UserCode { get; set; }
        public string Position { get; set; }
        public string EffectiveDate { get; set; }
        public string ExpiredDate { get; set; }
        public bool IsMasterCard { get; set; }
        public short Status { get; set; }
        public short CardType { get; set; }
        public bool Gender { get; set; }
        public short WorkType { get; set; }
        public string Address { get; set; }
        public string Nationality { get; set; }
        public string City { get; set; }
        public string HomePhone { get; set; }
        public string OfficePhone { get; set; }
        public string PostCode { get; set; }
        public string Note { get; set; }
        public int PermissionType { get; set; }
        public string NationalIdNumber { get; set; }
        public string Password { get; set; }
        public string PasswordCheck { get; set; }
        public string ApplyReason { get; set; }
        public List<CardModel> CardList { get; set; }
        public List<int> CategoryOptionIds { get; set; }
        public int AccessGroupId { get; set; }
        public int?  WorkingTypeId { get; set; }
        public string Avatar { get; set; }
        public int ApproverId1 { get; set; }
        public int ApproverId2 { get; set; }
        public int ApprovalStatus { get; set; }
        public string EmployeeNumber { get; set; }
        public List<DoorModel> DoorList { get; set; }
        public NationalIdCardModel NationalIdCard { get; set; }
    }

    public class UserDataModel : UserModel
    {
        public List<AccessGroupModelForUser> AccessGroups { get; set; }
        public IEnumerable<Category.Node> CategoryOptions { get; set; }
    }

    public class CardModel
    {
        public int Id { get; set; }
        public string CardId { get; set; }
        public int IssueCount { get; set; }
        public short CardStatus { get; set; }
        public string Description { get; set; }
        
        public int CardType { get; set; }

        public FaceModel FaceData { get; set; }
        public List<FingerPrintModel> FingerPrintData { get; set; }
        public string CreatedDate { get; set; }

    }

    public class UserCardModel
    {
       public UserModel UserModel { get; set; }
       public CardModel CardModel { get; set; }
    }


    public class FaceModel
    {
        public string LeftIrisImage { get; set; }
        public string RightIrisImage { get; set; }
        public string FaceImage { get; set; }
        public string FaceSmallImage { get; set; }
        public string LeftIrisCode { get; set; }
        public string RightIrisCode { get; set; }
        public string FaceCode { get; set; }
    }

    public class UserListModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserCode { get; set; }
        public string DepartmentName { get; set; }
        public string AccessGroupName { get; set; }
        public string EmployeeNo { get; set; }
        public string Position { get; set; }
        public string ExpiredDate { get; set; }
        public string EffectiveDate { get; set; }
        public short Status { get; set; }

        public string WorkTypeName { get; set; }

        public string Avatar { get; set; }

        public List<CardModel> CardList { get; set; }
        public List<CardModel> FaceList { get; set; }
        public List<CardModel> PlateNumberList { get; set; }

        public List<UserCategoryDataModel> CategoryOptions { get; set; }

        public string ApprovalStatus { get; set; }
        
        public string Email { get; set; }
        public string NationalIdNumber { get; set; }
        public string HomePhone { get; set; }
        public string Address { get; set; }
        public int AccountId { get; set; }
    }

    public class AccessibleUserModel
    {
        public int Id { get; set; }
        public string CardId { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string Department { get; set; }
        public string DepartmentName { get; set; }
        public string EmployeeNumber { get; set; }
        public string EmployeeNo { get; set; }
        public string ExpiredDate { get; set; }
        public string CardStatus { get; set; }
        public string Position { get; set; }
        public string WorkTypeName { get; set; }
        public string AccessGroupName { get; set; }
        public List<CardModel> CardList { get; set; }
        public List<CardModel> PlateNumberList { get; set; }
        public List<UserCategoryDataModel> CategoryOptions { get; set; }
    }

    public class PinPadModel
    {
        public string PinNumber { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }


    public class DoorModel
    {
        public int DoorId { get; set; }
        public int AccessTimeId { get; set; }
    }



    public class MultipleVisitorCardModel
    {
        /// <summary>
        /// Visitor's identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Model data for visitor's card
        /// </summary>
        public CardModel CardModel { get; set; }
    }


    /// <summary>
    /// Simple version of UserListModel
    /// </summary>
    public class UserListSimpleModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string UserCode { get; set; }
        public string EffectiveDate { get; set; }
        public string ExpiredDate { get; set; }
        public int AccessGroupId { get; set; }
        public int? WorkingTypeId { get; set; }
        public short WorkType { get; set; }
        public List<CardModel> CardList { get; set; }
    }

    public class UserWorkType
    {
        public int UserId { get; }
        public short WorkType { get; }

        public UserWorkType(short workType, int userId)
        {
            WorkType = workType;
            UserId = userId;
        }

        public override bool Equals(object obj)
        {
            return obj is UserWorkType other &&
                   WorkType == other.WorkType &&
                   UserId == other.UserId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(WorkType, UserId);
        }
    }



    public class UserInOutStatusListModel
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string WorkTypeName { get; set; }

        public string DepartmentName { get; set; }

        public string Reason { get; set; }
        
        public string MilitaryNumber { get; set; }

        public DateTime LastEventTime { get; set; }

        public List<CardModel> CardList { get; set; }

        public List<CardModel> PlateNumberList { get; set; }

    }


    public class UserGetConditionModel
    {
        public List<string> UserCodes { get; set; }

        public List<string> CardIds { get; set; }
    }
}
