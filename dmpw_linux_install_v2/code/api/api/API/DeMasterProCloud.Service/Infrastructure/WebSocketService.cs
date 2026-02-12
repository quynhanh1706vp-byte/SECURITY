using System;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataModel.DeviceSDK;
using Microsoft.Extensions.Logging;

namespace DeMasterProCloud.Service.Infrastructure;

public interface IWebSocketService
{
    void SendWebSocketToFE<T>(string type, int companyId, T data);
}
public class WebSocketService : IWebSocketService
{
    public void SendWebSocketToFE<T>(string type, int companyId, T data)
    {
        try
        {
            ApplicationVariables.SendMessageToAllClients(Helpers.JsonConvertCamelCase(new SDKDataWebhookModel()
            {
                Type = type,
                Data = data
            }), companyId);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + ex.StackTrace);
        }
    }
}