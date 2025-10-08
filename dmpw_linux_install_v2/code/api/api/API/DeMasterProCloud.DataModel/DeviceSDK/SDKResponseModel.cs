namespace DeMasterProCloud.DataModel.DeviceSDK;

public class SDKResponseModel<T>
{
    public string Message { get; set; }
    public bool StatusCode { get; set; }
    public T Data { get; set; }
}