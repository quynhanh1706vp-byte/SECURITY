using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Api
{
    public class TokenModel
    {
        public int Status { get; set; }
        public string HashQrCode { get; set; }
        public string AuthToken { get; set; }

        public string RefreshToken { get; set; }
        public short AccountType { get; set; }

        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string Logo { get; set; }
        public string DepartmentName { get; set; }
        public Dictionary<string, string> QueueService { get; set; }

        public string UserTimeZone { get; set; }
        public string UserLanguage { get; set; }
        public int UserPreferredSystem { get; set; }

        public int? AccountId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public int ExpireAccessToken { get; set; }
        public string ExpiredDate { get; set; }
        public bool LicenseVerified { get; set; }
        public bool EnableDepartmentLevel { get; set; }
        public int DefaultPaginationNumber { get; set; }
        public Dictionary<string, bool> PlugIn { get; set; }

        public Dictionary<string, Dictionary<string, bool>> Permissions { get; set; }
         
        // Login security notifications
        public bool PasswordChangeRequired { get; set; }
        public string PasswordChangeMessage { get; set; }
    }

    public class TemporaryTokenModel
    {
        public TemporaryTokenModel()
        {
            Meta = new MetaPage();
        }
        public int Status { get; set; }
        public string TemporaryToken { get; set; }
        public int? AccountId { get; set; }
        public bool LicenseVerified { get; set; }

        public List<CompanyApiDetailModel> Companies { get; set; }
        public MetaPage Meta { get; set; }
    }
    public class MetaPage
    {
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public int TotalUnRead { get; set; }
        public decimal TotalAmount { get; set; }
    }
}