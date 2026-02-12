using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DeMasterProCloud.DataModel.DeviceMessage
{
    public class DeviceMessageModel
    {
        [JsonIgnore]
        public int Id { get; set; }
        public int MessageId { get; set; }
        public string Content { get; set; }
        public string Remark { get; set; }
    }

    public class DeviceMessageListModel
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public string Content { get; set; }
        public string Remark { get; set; }
    }
}
