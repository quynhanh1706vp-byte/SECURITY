using System;

namespace DeMasterProCloud.DataAccess.Models
{
    public partial class Face
    {

        public int Id { get; set; }

        public int CompanyId { get; set; }
        public int? UserId { get; set; }

        public string LeftIrisImage { get; set; }
        public string RightIrisImage { get; set; }
        public string FaceImage { get; set; }
        public string FaceSmallImage { get; set; }
        public string LeftIrisCode { get; set; }
        public string RightIrisCode { get; set; }
        public string FaceCode { get; set; }

        public Company Company { get; set; }
        public User User { get; set; }

    }
}
