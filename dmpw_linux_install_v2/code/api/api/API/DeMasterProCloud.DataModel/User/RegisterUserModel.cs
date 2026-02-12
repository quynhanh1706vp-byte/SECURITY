using System;

namespace DeMasterProCloud.DataModel.User
{
    public class RegisterUserModel
    {
        public string FirstName { get; set; }
        public string Email { get; set; }
        public DateTime BirthDay { get; set; }
        public string Position { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public bool Gender { get; set; }
        public short WorkType { get; set; }
        public string Address { get; set; }
        public string Nationality { get; set; }
        public string City { get; set; }
        public string HomePhone { get; set; }
        public string OfficePhone { get; set; }
        public string PostCode { get; set; }
        public string Note { get; set; }
        public string NationalIdNumber { get; set; }
        public string Avatar { get; set; }
        public int ApproverId { get; set; }
    }

    public class RegisterUserInitModel
    {
        public string Logo { get; set; }
        public string Language { get; set; }
        public string Timezone { get; set; }
        public string CompanyName { get; set; }
    }

    public class ResultRegisterUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int UserId { get; set; }
    }
}