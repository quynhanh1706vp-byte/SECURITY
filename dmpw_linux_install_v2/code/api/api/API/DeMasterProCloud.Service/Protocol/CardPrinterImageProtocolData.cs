using System;

namespace DeMasterProCloud.Service.Protocol
{
    public class CardPrinterImageProtocolData : ProtocolData<CardPrinterImageDetail>
    {
        
    }

    public class CardPrinterImageDetail
    {
        public string PathImageFront { get; set; }
        public string PathImageBack { get; set; }
        public int CardId { get; set; }
        public DateTime UpdatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public bool Status { get; set; }
        public string DeviceAddress { get; set; }
    }
}