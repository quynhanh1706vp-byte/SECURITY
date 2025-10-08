using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using DeMasterProCloud.Common.Resources;
using Newtonsoft.Json;
using System.Collections.Generic;
using DeMasterProCloud.DataModel.Setting;
using DocumentFormat.OpenXml.Office2010.ExcelAc;

namespace DeMasterProCloud.DataModel.Company
{
    public class CompanyModel
    {
        [JsonIgnore]
        public int Id { get; set; }

        // [JsonIgnore]
        [Display(Name = nameof(CompanyResource.lblCompanyCode), ResourceType = typeof(CompanyResource))]
        public string Code { get; set; }

        [JsonIgnore]
        [Display(Name = nameof(CompanyResource.lblCompanyMiniLogo), ResourceType = typeof(CompanyResource))]
        public string MiniLogo { get; set; }

        public bool MiniLogoEnable { get; set; }

        [JsonIgnore]
        [Display(Name = nameof(CompanyResource.lblCompanyLogo), ResourceType = typeof(CompanyResource))]
        public string Logo { get; set; }

        public bool LogoEnable { get; set; }

        [Display(Name = nameof(CompanyResource.lblCompanyName), ResourceType = typeof(CompanyResource))]
        public string Name { get; set; }

        [Display(Name = nameof(CompanyResource.lblRemarks), ResourceType = typeof(CompanyResource))]
        public string Remarks { get; set; }

        //public bool RootFlag { get; set; }

        [Display(Name = nameof(CompanyResource.lblStatus), ResourceType = typeof(CompanyResource))]
        public short Status { get; set; }

        public bool AutoSyncUserData { get; set; }

        public string WebsiteUrl { get; set; }
        public string ContactWEmail { get; set; }
        public string Phone { get; set; }
        public string Industries { get; set; }
        public string Location { get; set; }
        public int EventLogStorageDurationInDb { get; set; }
        public int EventLogStorageDurationInFile { get; set; }
        public int CardBit { get; set; }
        public int LimitCountOfUser { get; set; }
        public bool EnableReCheckImageCamera { get; set; }
        public int TimeLimitCheckImageCamera { get; set; }
        public int TimeLimitStoredImage { get; set; }
        public int TimeLimitStoredVideo { get; set; }
        public List<LanguageDetailModel> ListLanguage { get; set; }
        public List<LanguageDetailModel> AllLanguage { get; set; }
    }

    public class CompanyModelOptions : CompanyModel
    {
        public List<int> CardBitOptions { get; set; }

        public List<bool> AutoSyncUserDataOptions { get; set; }
    }

    public class CompanyListModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        //public DateTime ExpiredDate { get; set; }
        //public string ExpiredTo { get; set; }

        //public string Admin { get; set; }

        //public string Status { get; set; }
        //public DateTime CreatedDate { get; set; }
        public string Createdon { get; set; }
        //public string EditUrl { get; set; }
        //public string DeleteUrl { get; set; }
        //public bool RootFlag { get; set; }
        public string Remarks { get; set; }
    }

    public class CompanyDataModel
    {
        public IEnumerable<SelectListItemModel> BuildingItems { get; set; }
        public int DefaultBuilding { get; set; }
        public IEnumerable<SelectListItemModel> ActiveTzItems { get; set; }
        public int DefaultActiveTz { get; set; }
        public IEnumerable<SelectListItemModel> PassiveTzItems { get; set; }
        public int DefaultPassiveTz { get; set; }
    }

    //public class CompanyModelForAccount
    //{
    //    public int Id { get; set; }
    //    public String Name { get; set; }
    //}


    public class EncryptSettingModel
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class ExpiredPWSettingModel
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
        public int Period { get; set; }
    }
}
