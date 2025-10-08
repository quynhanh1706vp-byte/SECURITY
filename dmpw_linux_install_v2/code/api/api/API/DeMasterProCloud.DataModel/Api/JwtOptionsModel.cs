namespace DeMasterProCloud.DataModel.Api
{
    /// <summary>
    /// Class represent for jwt section on appseting.json file
    /// </summary>
    public class JwtOptionsModel
    {
        public string SecretKey { get; set; }
        public int ExpiryMinutes { get; set; }
        public int expiryRefreshToken { get; set; }
        public string Issuer { get; set; }
    }
}