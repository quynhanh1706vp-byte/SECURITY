using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DeMasterProCloud.Service.Protocol
{
    public class PrometheusMetricData
    {
        public string Topic { get; set; }
        public string DeviceAddress { get; set; }
        public double TotalMilliseconds { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }
    }
}
