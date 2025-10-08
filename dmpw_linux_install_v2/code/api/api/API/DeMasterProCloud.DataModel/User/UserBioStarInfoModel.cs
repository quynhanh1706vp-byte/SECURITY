using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.User
{
    public class UserBioStarInfoModel
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public byte[] Avatar { get; set; }
        public long StartTime { get; set; }
        public string EffectiveDate { get; set; }
        public long EndTime { get; set; }
        public string ExpiredDate { get; set; }
        public int AccessGroupId { get; set; }
        public string AccessGroupName { get; set; }
        public int NumOfFinger { get; set; }
        public int NumOfCard { get; set; }
        public int NumOfFace { get; set; }
        public List<CardBioStarModel> Cards { get; set; }
        public List<FingerBioStarModel> Fingers { get; set; }
    }
    
    public class CardBioStarModel
    {
        public string CardId { get; set; }
        public short Type { get; set; }
    }

    public class FingerBioStarModel
    {
        public List<string> Templates { get; set; }
    }
}