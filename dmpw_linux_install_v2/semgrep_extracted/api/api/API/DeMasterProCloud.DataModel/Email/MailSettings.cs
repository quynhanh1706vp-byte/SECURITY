namespace DeMasterProCloud.DataModel.Email
{
    public class MailDevelopSettings
    {
        public string From { get; set; }
        public string To { get; set; }
    }

    public class MailSettings
    {
        public string From { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public bool EnableSsl { get; set; }
        public bool DefaultCredentials { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
