using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMasterProCloud.DataModel.FirmwareVersion
{
    public class FirmwareVersionModel
    {
        public int Id { get; set; }
        public string Version { get; set; }
        public string FileName { get; set; }
        public short DeviceType { get; set; }
        public string LinkFile { get; set; }
        public string Note { get; set; }
    }

    public class FirmwareVersionListModel
    {
        public int Id { get; set; }
        public string Version { get; set; }
        public string FileName { get; set; }
        public short DeviceType { get; set; }
        public string Note { get; set; }
        public string CreatedOn { get; set; }
        public string LinkFile { get; set; }
    }

    public class FirmwareVersionDeviceModel
    {
        public string DeviceAddress { get; set; }
        public short DeviceType { get; set; }
        // 0 = In , 1 = Out
        public short? RoleReader0 { get; set; } = 0;
        public short? RoleReader1 { get; set; } = 1;
        public string VersionReader0 { get; set; }
        public string VersionReader1 { get; set; }
        public string ExtraVersion { get; set; }
        public string UrlUploadFileResponse { get; set; }
        public string Sender { get; set; }
        public string LinkFile { get; set; }
        public string Version { get; set; }
        public string Target { get; set; }
        public string FwType { get; set; }

    }
}
