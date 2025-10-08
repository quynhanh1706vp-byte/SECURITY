using System.Collections.Generic;
using DeMasterProCloud.DataModel.User;

namespace DeMasterProCloud.Service.Protocol
{
    public class RequestAllUserProtocolData : ProtocolData<object>
    {
    }

    public class RequestAllUserResponseProtocolData : ProtocolData<RequestAllUserResponseDetail>
    {
    }

    public class RequestAllUserResponseDetail
    {
        public int FrameIndex { get; set; }
        public int TotalIndex { get; set; }
        public int Total { get; set; }
        public List<UserBioStarInfoModel> Users { get; set; }
    }

    public class AllUserFromEbknProtocolData : ProtocolData<AllUserFromEbknDetail>
    {
        
    }

    public class AllUserFromEbknDetail
    {
        public int FrameIndex { get; set; }
        public int TotalIndex { get; set; }
        public int Total { get; set; }
        public List<UserInfoFromEbkn> Users { get; set; }
    }

    public class UserInfoFromEbkn
    {
        // this is ID of ebkn device but is UserCode of DMPW
        public string Id { get; set; }
        public string Name { get; set; }
        public short Role { get; set; }
        public string PassCode { get; set; }
        public string FaceData { get; set; }
        public List<string> Fingerprints { get; set; }
        public string Photo { get; set; }
    }

    public class AllUserFromAratekProtocolData : ProtocolData<AllUserFromAratekDetail>
    {
        
    }

    public class AllUserFromAratekDetail
    {
        public int FrameIndex { get; set; }
        public int TotalIndex { get; set; }
        public int Total { get; set; }
        public List<UserInfoFromAratek> Users { get; set; }
    }


    public class UserInfoFromAratek
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public long StartTime { get; set; }
        public string EffectiveDate { get; set; }
        public long EndTime { get; set; }
        public string ExpiredDate { get; set; }
        public int AccessGroupId { get; set; }
        public string AccessGroupName { get; set; }
        public int NumOfFinger { get; set; }
        public int NumOfCard { get; set; }
        public int NumOfFace { get; set; }
        public List<CardAratekModel> Cards { get; set; }
        public List<FingerAratekModel> Fingers { get; set; }
    }

    public class CardAratekModel
    {
        public string CardId { get; set; }
        public short Type { get; set; }
    }

    public class FingerAratekModel
    {
        public List<string> Templates { get; set; }
    }
}