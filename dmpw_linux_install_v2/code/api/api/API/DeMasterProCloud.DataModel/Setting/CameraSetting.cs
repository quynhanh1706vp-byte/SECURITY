namespace DeMasterProCloud.DataModel.Setting
{
    public class CameraSetting
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int PlaceId { get; set; }
        public string Server { get; set; }
        public bool IsEnableAutoSync { get; set; } = true;
    }
}