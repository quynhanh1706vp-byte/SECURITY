using System;

namespace DeMasterProCloud.DataModel.User
{
    public class UserHVQYModel
    {
        public int MaVaoRa { get; set; }
        public string MaSoHocVien { get; set; }
        public int Tuan { get; set; }
        public int LoaiDangKi { get; set; }
        public DateTime Ngay { get; set; }
        public bool Khoa { get; set; }
        public string NoiDangKi { get; set; }
        public string LyDo { get; set; }
        public string VaoMuon { get; set; }
        public string CCCD { get; set; }
    }
    public class HVQYRequestEvent
    {
        public string MaSoHocVien { get; set; }
        public int LoaiDangKi { get; set; }
        public int Tuan { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}