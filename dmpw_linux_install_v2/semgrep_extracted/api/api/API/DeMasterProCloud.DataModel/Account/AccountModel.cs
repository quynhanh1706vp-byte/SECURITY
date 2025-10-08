using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using DeMasterProCloud.Common.Resources;
using Newtonsoft.Json;
using DeMasterProCloud.DataModel.Company;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataModel.Setting;

namespace DeMasterProCloud.DataModel.Account
{
    public class AccountTimeZoneModel
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string TimeZone { get; set; }
    }
    public class AccountModel
    {
        //[JsonIgnore]
        public int Id { get; set; }
        public string Username { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public int? CompanyId { get; set; }
        public bool RootFlag { get; set; }
        public short Role { get; set; }
        //public string Remarks { get; set; }
        public short Status { get; set; } = 1;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TimeZone { get; set; }
        public string Language { get; set; }
        public int PreferredSystem { get; set; }
        public List<LanguageDetailModel> AllLanguage { get; set; }
    }
    public class AccountDataModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public int CompanyId { get; set; }
        public bool RootFlag { get; set; }
        public int Role { get; set; }
        public short Status { get; set; }
        public string TimeZone { get; set; }
        public bool IsCurrentAccount { get; set; }
        public IEnumerable<CompanyListModel> CompanyIdList { get; set; }
        public IEnumerable<EnumModel> RoleList { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }
    }


    public class AccountListModel
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }

        /// <summary>
        /// This variable has the same value with 'Username'.
        /// 'Username' variable will be exchanged to 'FirstName'.
        /// </summary>
        public string FirstName { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public string CompanyName { get; set; }
        public List<string> CompanyNames { get; set; }
        public string TimeZone { get; set; }
        public string Department { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DepartmentName { get; set; }
        public string Position { get; set; }
        //public string Remarks { get; set; }
    }

    //public class ForgotPasswordModel
    //{
    //    public string CompanyCode { get; set; }
    //    public string Username { get; set; }
    //}

    public class ContactModel
    {
        public int CompanyId { get; set; }
        public string Contact { get; set; }
    }

    public class AccountAvatarModel
    {
        public string Avatar { get; set; }
    }

    public class ChangePasswordModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
    
    public class ChangePasswordLoginModel
    {
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}
