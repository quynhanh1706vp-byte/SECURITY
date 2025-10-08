using System;
using System.Collections.Generic;
using System.Text;

namespace DeMasterProCloud.DataModel
{
    public class ResponseStatus
    {
        public string message { get; set; }
        public bool statusCode { get; set; }
        public object data { get; set; }
    }
}
