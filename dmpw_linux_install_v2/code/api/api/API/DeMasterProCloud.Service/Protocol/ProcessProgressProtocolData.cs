using System;
using System.Collections.Generic;
using System.Text;
using DeMasterProCloud.Common.Infrastructure;

namespace DeMasterProCloud.Service.Protocol
{
    public class ProcessProgressProtocolData : ProtocolData<ProcessProgressDataDetail>
    {

        /// <summary>
        /// Make long process progress protocol data
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static string MakeLongProcessProgressMessage(string processId,
                                                            decimal progress,
                                                            string name,
                                                            string sender)
        {
            var progressData = new ProcessProgressDataDetail
            {
                ProcessId = processId,
                Progress = progress.ToString("0.00"),
                Name = name
            };
            var progressProtocol = new ProcessProgressProtocolData
            {
                MsgId = Guid.NewGuid().ToString(),
                Sender = sender,
                Type = Constants.Protocol.LongProcessProgress,
                Data = progressData
            };
            string message = progressProtocol.ToString();
            return message;
        }
    }

    public class ProcessProgressDataDetail
    {
        public string ProcessId { get; set; }
        public string Progress { get; set; }
        public string Name { get; set; }
    }

}
