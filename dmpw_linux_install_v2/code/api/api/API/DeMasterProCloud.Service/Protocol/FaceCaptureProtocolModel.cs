using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMasterProCloud.Service.Protocol
{
    class FaceCaptureProtocolModel : ProtocolData<FaceCaptureModelReponseDetail>
    {
    }

    public class FaceCaptureModelReponseDetail
    {
        public string Status { get; set; }
        public string DeviceType { get; set; }

        public FaceDataList FaceData { get; set; }
    }

    public class FaceDataList
    {
        public string LeftIrisImage { get; set; }
        public string RightIrisImage { get; set; }
        public string FaceImage { get; set; }
        public string FaceSmallImage { get; set; }
        public string LeftIrisCode { get; set; }
        public string RightIrisCode { get; set; }
        public string FaceCode { get; set; }
    }

    class FaceCaptureRequestModel : ProtocolData<FaceCaptureModelRequestDetail>
    {
    }

    public class FaceCaptureModelRequestDetail
    {
        public int Duration { get; set; }
    }



    /// <summary>
    /// Send face data Id value to webapp
    /// </summary>
    public class SendFaceDataModelToFE : ProtocolData<SendFaceDataModel>
    {
    }

    public class SendFaceDataModel
    {
        //public string FaceId { get; set; }

        public FaceDataList FaceData { get; set; }
    }




    /// <summary>
    /// To receive face registration message from device
    /// </summary>
    class FaceRegisterProtocolModel : ProtocolData<FaceRegisterModelReponseDetail>
    {
    }

    public class FaceRegisterModelReponseDetail
    {
        public string DeviceAddress { get; set; }
        public string DeviceType { get; set; }
        public string CardId { get; set; }
        /// <summary>
        /// Flag to distinguish Update or New.
        /// (Why did you design this as a string????? To Compare by string is not good..)
        /// </summary>
        /// <example>
        /// "N" : New
        /// "U" : Update
        /// </example>
        public string UpdateFlag { get; set; }
        public FaceDataList FaceData { get; set; }
    }


    /// <summary>
    /// For sending face Id value to register device
    /// </summary>
    public class FaceRegisterResponseModel : ProtocolData<FaceRegisterResponseDetail>
    {
    }

    public class FaceRegisterResponseDetail
    {
        public string FaceId { get; set; }
        public string Status { get; set; }
    }
}
