using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.RabbitMq
{
    public class RabbitMqAccountInfo
    {
        public string hashing_algorithm { get; set; }
        public object limits { get; set; }
        public string name { get; set; }
        public string password_hash { get; set; }
        public object tags { get; set; }
        public List<string> list_tags { get; set; }
    }
}