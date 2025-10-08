using System;
using System.Collections.Generic;
using System.Text;

namespace DeMasterProCloud.Service.Protocol
{
    public class DeviceUploadFileProtocolData : ProtocolData<DeviceUploadFileDetail>
    {
    }

    public class DeviceUploadFileDetail
    {
        public int FrameIndex { get; set; }
        public int TotalIndex { get; set; }
        public string Extension { get; set; }
        public string Target { get; set; }
        public string FwType { get; set; }
        public string Data { get; set; }

        public string FileName { get; set; }
    }

    public class DeviceUploadFileResponse : ProtocolData<DeviceUploadFileResponseDetail>
    {
    }

    public class DeviceUploadFileResponseDetail
    {
        public int FrameIndex { get; set; }
        public int TotalIndex { get; set; }
        public string Status { get; set; }
    }

    public class MainFirmwareProtocolData : ProtocolData<MainFirmwareHeader>
    {
    }

    public class MainFirmwareHeader
    {
        public string Command { get; set; }
        public string LinkFile { get; set; }
        public string Version { get; set; }
        public Option Options { get; set; }
    }

    public class Option
    {
        public string Target { get; set; }
        public string FwType { get; set; }
    }
}
