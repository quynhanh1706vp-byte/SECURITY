using System.Collections.Generic;

namespace DeMasterProCloud.Service.Protocol
{
    public class DeviceConnectionStatusProtocolData : ProtocolData<DeviceConnectionStatusDetail>
    {
    }

    public class DeviceConnectionStatusDetail
    {
        public string DeviceAddress { get; set; }
        public int Status { get; set; }
        public string IpAddress { get; set; }
        public string DeviceType { get; set; }
        public string MacAddress { get; set; }
    }

    public class DeviceStatusProtocolsData : ProtocolData<List<DeviceStatusDetail>> { }

    public class DeviceStatusProtocolData : ProtocolData<DeviceStatusDetail> { }

    public class DeviceStatusDetail
    {
        public string DeviceAddress { get; set; }
        public string IpAddress { get; set; }
        public int Status { get; set; }
        public string DeviceType { get; set; }
        public int EventCount { get; set; }
        public int UserCount { get; set; }
        public int FromDbIdNumber { get; set; }

        /// <summary>
        /// User's card count value in DB.
        /// This value can effect to performance (Because BE should search DB about all device per 2 mins)
        /// </summary>
        public int DbIdCount { get; set; }
        public string LastCommunicationTime { get; set; }
        public string DoorStatus { get; set; }
        /// <summary>
        /// Id value of DoorStatus
        /// </summary>
        public int DoorStatusId { get; set; }
        public string Version { get; set; }
        public string InReader { get; set; }
        public string OutReader { get; set; }
        public string NfcModule { get; set; }
        
        ///// <summary>
        ///// Background color of door status
        ///// </summary>
        //public string BackgroundColor { get; set; }


        //public DoorStatusModel DoorStatusInfo { get; set; }
    }


    public class DoorStatusModel
    {
        /// <summary>
        /// Id value of DoorStatus
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of DoorStatus
        /// </summary>
        public string Name { get; set; }
        

        /// <summary>
        /// Background color
        /// </summary>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Font color
        /// </summary>
        public string FontColor { get; set; }
    }
}
