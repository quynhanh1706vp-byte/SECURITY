using System;
using System.Collections.Generic;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Setting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace DeMasterProCloud.DataModel.RabbitMq
{
    public class DataQueueModel
    {
        public string MsgId { get; set; }
        public string MessageType { get; set; }
        public string Sender { get; set; }
        public int DeviceId { get; set; }
        public int ControllerId { get; set; }
        public string DeviceAddress { get; set; }
        
        // index process
        public int MessageIndex { get; set; }
        public int MessageTotal { get; set; }
    }
    public class EbknQueueModel : DataQueueModel
    {
        public int CompanyId { get; set; }
    }
    public class InstructionQueueModel : DataQueueModel
    {
        public string Command { get; set; }
        public string UserName { get; set; }
        public DateTime OpenUtilTime { get; set; }
        public int OpenPeriod { get; set; }
        public string Target { get; set; }
        public string FwType { get; set; }
        public LocalMqttSetting LocalMqtt { get; set; }
        public bool IsSaveSystemLog { get; set; }
        public string LinkFile { get; set; }
        public string FileName { get; set; }
        public string Version { get; set; }
        public string Serial { get; set; }
        public string Key { get; set; }
        public string ActivationCode { get; set; }
        public byte[] FileData { get; set; }
    }

    public class LoadDeviceInfoQueueModel : DataQueueModel
    {
        
    }

    public class ConfigQueueModel : DataQueueModel
    {
        public bool UseStaticQrCode { get; set; }
    }

    public class UpdateTimezoneQueueModel : DataQueueModel
    {
        
    }
    
    public class DeleteTimezoneQueueModel : DataQueueModel
    {
        public AccessTime Timezone { get; set; }
    }

    public class HolidayQueueModel : DataQueueModel
    {
        
    }

    public class CompanyConfigQueueModel : DataQueueModel
    {
        public int CompanyId { get; set; }
    }

    public class DeviceMessageQueueModel : DataQueueModel
    {
        
    }
    
    public class MealServiceQueueModel : DataQueueModel
    {
        public int MealServiceId { get; set; }
    }

    public class UploadFileQueueModel : DataQueueModel
    {
        public IFormFile File { get; set; }
        public string Language { get; set; }
    }

    public class UserInfoQueueModel : TotalUserInfoQueueModel
    {
        public List<int> UserIds { get; set; }
        public List<int> VisitIds { get; set; }
        public List<int> CardIds { get; set; }
        public List<int> DeviceIds { get; set; }
        public List<int> OverwriteFloorIndex { get; set; } = null;
    }

    public class TotalUserInfoQueueModel : DataQueueModel
    {
        public TotalUserInfoQueueModel()
        {
            // Default value is null
            TotalData = null;
        }

        public TotalData TotalData { get; set; }
    }

    /// <summary>
    /// This model is for the improving performance for sending user data to device.
    /// </summary>
    public class TotalData
    {
        /// <summary>
        /// list of card identifier to be sent to devices
        /// </summary>
        public List<int> CardIds { get; set; }

        /// <summary>
        /// list of device identifiers that need to receive user data
        /// </summary>
        public List<int> DeviceIds { get; set; }
    }


    public class VisitInfoQueueModel : DataQueueModel
    {
        public List<int> UserIds { get; set; }
        public List<int> VisitIds { get; set; }
        public List<int> CardIds { get; set; }
    }
    public class AccessGroupQueueModel : DataQueueModel
    {
        
    }
    
    public class VehicleInfoQueueModel : DataQueueModel
    {
        public List<int> UserIds { get; set; }
        public List<int> VisitIds { get; set; }
        public List<int> CardIds { get; set; }
    }

    public class InstructionCommonModel : DataQueueModel
    {
        public List<int> CardIds { get; set; }
        public List<int> UserIds { get; set; }
        public List<int> VisitIds { get; set; }
        public List<int> CardFilterIds { get; set; }
        public string CompanyCode { get; set; }
        public List<int> DeviceIds { get; set; }
        public short? DeviceType { get; set; } = null;

        public InstructionCommonModel Clone()
        {
            return JsonConvert.DeserializeObject<InstructionCommonModel>(JsonConvert.SerializeObject(this));
        }
    }
}