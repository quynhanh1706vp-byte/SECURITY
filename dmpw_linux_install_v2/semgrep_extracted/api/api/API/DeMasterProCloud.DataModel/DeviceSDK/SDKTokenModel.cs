namespace DeMasterProCloud.DataModel.DeviceSDK;

public class SDKLoginModel
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class SDKTokenModel
{
    public int Status { get; set; }
    public string AuthToken { get; set; }
    public string RefreshToken { get; set; }
    public string FullName { get; set; }
    public int ExpireAccessToken { get; set; }
}