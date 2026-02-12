using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace DeMasterProCloud.Common.Infrastructure
{
    public class ApplicationVariables
    {
        // init configuration
        public static IConfiguration Configuration;
        public static ILoggerFactory LoggerFactory;
        public static IHostingEnvironment Env;
        public static string TempPasswordSetting;
        
        // Dictionary that store queue take face picture with camera
        // Key is cameraId, value is Avatar and Time take a picture
        public static Dictionary<string, QueueTakeFacePicture> QueueTakeFacePictures = new Dictionary<string, QueueTakeFacePicture>();
        
        // rabbit mq connection
        private static Dictionary<short, IConnection> RabbitMqConnections = new Dictionary<short, IConnection>();
        public static IConnection GetAvailableConnection(IConfiguration configuration, short type = 0)
        {
            return null;
        }

        public static void SendMessageToAllClients(string message, int companyId = -1)
        {
            try
            {
                foreach (var key in ClientSockets.Keys)
                {
                    if (companyId < 0 || ClientSockets[key].CompanyId == companyId)
                    {
                        ClientSockets[key].Client.SendAsync(
                            Encoding.UTF8.GetBytes(message),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        
        // SDK Variable
        public static string SDKToken = string.Empty;
        public static string SDKRefreshToken = string.Empty;
        public static string SDKUserName = Constants.SDKDevice.SenderDefault;
        
        // Websocket Client
        public static Dictionary<string, WebSocketItem> ClientSockets = new();
    }

    public class QueueTakeFacePicture
    {
        public DateTime Time { get; set; }
        public string Avatar { get; set; }
    }

    public class WebSocketItem
    {
        public WebSocket Client { get; set; }
        public int CompanyId { get; set; }
    }
}
