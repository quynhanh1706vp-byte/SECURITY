namespace DeMasterProCloud.DataModel.RabbitMq
{
    public class RabbitMqSettingModel
    {
        public string Host { get; set; }
        public string VirtualHost { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}