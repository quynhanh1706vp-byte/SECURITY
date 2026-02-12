using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using Newtonsoft.Json;

namespace DeMasterProCloud.DataModel.Holiday
{
    public class HolidayModel
    {
        [JsonIgnore]
        public int Id { get; set; }
        [Display(Name = nameof(HolidayResource.lblHolidayName), ResourceType = typeof(HolidayResource))]
        public string Name { get; set; }
        public int CompanyId { get; set; }
        [Display(Name = nameof(HolidayResource.lblHolidayType), ResourceType = typeof(HolidayResource))]
        public int Type { get; set; }
        [Display(Name = nameof(HolidayResource.lblStartDate), ResourceType = typeof(HolidayResource))]
        public string StartDate { get; set; }
        [Display(Name = nameof(HolidayResource.lblEndDate), ResourceType = typeof(HolidayResource))]
        public string EndDate { get; set; }
        public bool Recursive { get; set; }
        public string Remarks { get; set; }
        public IEnumerable<EnumModel> HolidayTypeItems { get; set; }

    }

    public class HolidayListModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public short Type { get; set; }
        public string HolidayType { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool Recursive { get; set; }
        public string RecursiveDisp { get; set; }
        public string Remarks { get; set; }
    }

    public class HolidayInfoModel
    {
        public HolidayInfoModel()
        {
            Data = new List<HolidayDetail>();
        }
        public string MsgId { get; set; }
        public List<HolidayDetail> Data { get; set; }
    }

    public class HolidayDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string HolidayType { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int Recursive { get; set; }
    }
}
