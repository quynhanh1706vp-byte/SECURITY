using System;
using System.Collections.Generic;
using DeMasterProCloud.DataAccess.Models;

namespace DeMasterProCloud.Service.Protocol
{
    /// <summary>
    /// Sending Vehicle data to device
    /// </summary>
    public class VehicleProtocolData : ProtocolData<VehicleProtocolHeaderData>
    {
        public VehicleProtocolData()
        {
        }
    }

    public class VehicleProtocolHeaderData
    {
        public VehicleProtocolHeaderData()
        {
            Users = new List<VehicleProtocolDetailData>();
        }
        public int Total { get; set; }

        public int UpdateFlag { get; set; }

        public int FrameIndex { get; set; }
        public int TotalIndex { get; set; }

        public List<VehicleProtocolDetailData> Users { get; set; }
    }

    public class VehicleProtocolDetailData
    {
        public long Id { get; set; }
        public string PlateNumber { get; set; }
        public string Rfid { get; set; }
        public int UserId { get; set; }
        public bool UseCard { get; set; }
        public string EffectiveDate { get; set; }
        public string ExpiredDate { get; set; }
        public bool IsMaster { get; set; }

        public int VehicleRule { get; set; }
        public int VehicleRuleOption { get; set; }
    }



    /// <summary>
    /// vehicle response protocol data
    /// </summary>
    public class VehicleResponseProtocolData : ProtocolData<VehicleResponseHeaderData>
    {
    }

    public class VehicleResponseHeaderData
    {
        public VehicleResponseHeaderData()
        {
            Users = new List<VehicleResponseDetailData>();
        }
        public int Total { get; set; }
        public List<VehicleResponseDetailData> Users { get; set; }
    }

    public class VehicleResponseDetailData
    {
        public string PlateNumber { get; set; }
    }

}
