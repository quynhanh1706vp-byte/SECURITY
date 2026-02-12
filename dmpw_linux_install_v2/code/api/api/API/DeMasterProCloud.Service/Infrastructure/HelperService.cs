using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.DeviceSDK;
using DeMasterProCloud.Repository;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Constants = DeMasterProCloud.Common.Infrastructure.Constants;
using Message = FirebaseAdmin.Messaging.Message;

namespace DeMasterProCloud.Service.Infrastructure
{
    public class HelperService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        
        public HelperService(IConfiguration configuration)
        {
            if (configuration == null)
                _configuration = ApplicationVariables.Configuration;
            else
                _configuration = configuration;
            
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<HelperService>();
        }
        
        public async Task PushNotificationSettingByEventLogsAsync(IcuDevice icuDevice, List<EventLog> eventLogs)
        {
            // init
            IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
            INotificationService notificationService = new NotificationService(unitOfWork, _configuration);
            
            if (!icuDevice.CompanyId.HasValue)
            {
                // icu not assign to company
                unitOfWork.Dispose();
                return;
            }
            else
            {
                if (icuDevice.Company == null)
                {
                    icuDevice.Company = unitOfWork.CompanyRepository.GetById(icuDevice.CompanyId.Value);
                }

                if (icuDevice.Company == null)
                {
                    // wrong query company
                    unitOfWork.Dispose();
                    return;
                }
            }
            
            var emailSetting = unitOfWork.SettingRepository.GetByKey(Constants.Settings.ListUserToNotificationEmail, icuDevice.CompanyId.Value);
            var emails = JsonConvert.DeserializeObject<List<string>>(emailSetting.Value);
            if (emails == null || !emails.Any())
            {
                // email notification empty
                Console.WriteLine("[email notification empty]");
                unitOfWork.Dispose();
                return;
            }
                    
            var eventNotifySetting = unitOfWork.SettingRepository.GetByKey(Constants.Settings.EventListPushNotification, icuDevice.CompanyId.Value);
            var stringEventTypes = JsonConvert.DeserializeObject<List<string>>(eventNotifySetting.Value);
            if (stringEventTypes == null || !stringEventTypes.Any())
            {
                // event type notification empty
                Console.WriteLine("[event type notification empty]");
                unitOfWork.Dispose();
                return;
            }
            
            // account list
            var accounts = unitOfWork.AppDbContext.Account
                .Include(m => m.CompanyAccount)
                .Where(m => emails.Contains(m.Username) && m.CompanyAccount.Any(n => n.CompanyId == icuDevice.CompanyId));

            if (!accounts.Any())
            {
                // accounts null
                Console.WriteLine("[accounts null]");
                unitOfWork.Dispose();
                return;
            }
            
            foreach (var eventLogDetail in eventLogs)
            {
                try
                {
                    if (stringEventTypes.Contains(eventLogDetail.EventType.ToString()))
                    {
                        foreach (var account in accounts)
                        {
                            var accountCultureInfo = new CultureInfo(account.Language);
                            var messaging = FirebaseMessaging.DefaultInstance;
                            
                            string body = string.Format(EventLogResource.ResourceManager.GetString("msgPushNotificationToManager", accountCultureInfo), GetEventDescription(eventLogDetail.EventType, account.Language), icuDevice.Name);
                            if (messaging != null)
                            {
                                var topic = $"{icuDevice.Company.Code}_{account.Id}";
                                var fbMessage = new Message()
                                {
                                    Notification = new FirebaseAdmin.Messaging.Notification
                                    {
                                        Title = GetEventDescription(eventLogDetail.EventType, account.Language),
                                        Body = body,
                                    },
                                    Topic = topic,
                                    Data = new Dictionary<string, string>()
                                    {
                                        {"type", "warning"},
                                    }
                                };
                                
                                var res = await messaging.SendAsync(fbMessage);
                            
                                notificationService.AddNotification(new DeMasterProCloud.DataAccess.Models.Notification()
                                {
                                    CompanyId = icuDevice.CompanyId.Value,
                                    Type = (short)NotificationType.NotificationAccess,
                                    CreatedOn = DateTime.Now,
                                    Status = false,
                                    ReceiveId = account.Id,
                                    Content = fbMessage.Notification.Body,
                                    RelatedUrl = $"/event-log?id={eventLogDetail.Id}",
                                }, (short)NotificationType.NotificationAccess);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    _logger.LogError($"Push notification failed: {JsonConvert.SerializeObject(eventLogDetail)}");
                }
            }

            unitOfWork.Dispose();
        }

        public string GetEventDescription(int eventType, string language)
        {
            try
            {
                var enumName = Enum.GetName(typeof(EventType), eventType);
                string result = EventLogResource.ResourceManager.GetString($"lbl{enumName}", new CultureInfo(language));
                if (string.IsNullOrEmpty(result))
                {
                    return ((EventType)eventType).GetDescription();
                }
                else
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}