using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using DeMasterProCloud.Common.Infrastructure;

namespace DeMasterProCloud.DataModel.SystemLog
{
    public class SystemLogModel
    {
        public SystemLogModel()
        {
            ObjectTypeItems = new List<SelectListItem>();
            ActionItems = new List<SelectListItem>();
            CompanyItems = new List<SelectListItem>();
        }
        public string OpeDateFrom { get; set; }
        public string OpeDateTo { get; set; }
        public string OpeTimeFrom { get; set; }
        public string OpeTimeTo { get; set; }
        public int? ObjectType { get; set; }
        public int? Action { get; set; }
        public int? Company { get; set; }
        public List<SelectListItem> ObjectTypeItems { get; set; }
        public List<SelectListItem> ActionItems { get; set; }
        public List<SelectListItem> CompanyItems { get; set; }
    }


    public class SystemLogListModel
    {
        public long Id { get; set; }
        public string OperationTime { get; set; }
        public string UserAccount { get; set; }
        public string OperationType { get; set; }
        public string Action { get; set; }
        public string OperationAction { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }


        public bool Htmlparse { get; set; }
    }

    public class SystemLogPdfModel : SystemLogListModel
    {
        public string Company { get; set; }

    }


    public class SystemLogContent
    {
        [JsonProperty(PropertyName = "system_log_type")]
        public SystemLogType SystemLogType { get; set; }
        [JsonProperty(PropertyName = "action_log_type")]
        public ActionLogType ActionLogType { get; set; }
        public string Name { get; set; }
        public List<long> Ids { get; set; }

        public override string ToString()
        {
            switch (ActionLogType)
            {
                case ActionLogType.Login:
                case ActionLogType.Logout:
                    return $"{ActionLogType.GetDescription()}: {Name}";
                case ActionLogType.DeleteMultiple:
                    return $"{ActionLogType.GetDescription()} {SystemLogType.GetDescription()}";
                default:
                    return $"{ActionLogType.GetDescription()} {SystemLogType.GetDescription()}: {Name}";
            }
        }
    }

    public class SystemLogIdContent
    {
        public int Id { get; set; }
        [JsonProperty(PropertyName = "assigned_ids")]
        public List<int> AssignedIds { get; set; }
    }

    public class SystemLogOperationType
    {
        public List<SelectListItem> Data { get; set; }
    }

    public class SystemLogAction
    {
        public List<SelectListItem> Data { get; set; }
    }

    public class SystemLogOperationTime
    {
        public string OpeDateFrom { get; set; }
        public string OpeDateTo { get; set; }
        public string OpeTimeFrom { get; set; }
        public string OpeTimeTo { get; set; }
    }
}
