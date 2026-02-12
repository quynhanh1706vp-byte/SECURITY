using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.DeviceSDK;
using DeMasterProCloud.DataModel.Notification;
using DeMasterProCloud.Repository;
using DeMasterProCloud.Service.Protocol;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Constants = DeMasterProCloud.Common.Infrastructure.Constants;
using Notification = DeMasterProCloud.DataAccess.Models.Notification;

namespace DeMasterProCloud.Service
{
    public interface INotificationService
    {
        void SendMessage(string messageType, string notificationType, string msgUser, string msgBody, int companyId, bool keep = false, string relatedUrl = "");
        void PushingNotificationToUser(string type, int id, string title, string content, int receiveId, int companyId);
        List<NotificationData> GetNotifications(int companyId, int userId, string sortColumn, string sortDirection, string filter, int pageNumber, int pageSize,
            out int totalRecords, out int recordsFiltered, string firstAccessTime, string lastAccessTime, out int totalUnread);
        ResponseStatus UpdateStatus(int Id, NotificationUpdate model);
        ResponseStatus DeleteNotification(int id);
        ResponseStatus AddNotification(Notification model, int Type, IModel channel = null);
        Notification GetNotificationById(int id);
        ResponseStatus UpdateMultipleStatus(List<int> lstStatus, bool Status);
        ResponseStatus DeleteMultipleStatus(List<int> lstId);
        ResponseStatus DeleteAllStatus(int companyId, int userId);
        ResponseStatus UnReadAll(int companyId, int userId);
        Notification MappingNoti(int companyId, int receiveId, int notificationType, string resourceName, string resourceParam, string relatedUrl);
        bool CreateNotificationToUsers(NotificationNoticeModel model, int companyId);
        NoticeModel GetDashboardNotice(int accountId, int companyId);

        /// <summary>
        /// Get dashboard notice.
        /// </summary>
        /// <param name="companyId"> company identifier </param>
        /// <returns></returns>
        NoticeModel GetDashboardNotice(int companyId);

        /// <summary>
        /// Add Dashboard notification
        /// </summary>
        /// <param name="notice"> contents of notice </param>
        /// <param name="companyId"> company identifier </param>
        /// <returns></returns>
        ResponseStatus AddDashBoardNotice(string notice, int companyId);

        /// <summary>
        /// Update dashboard notice
        /// </summary>
        /// <param name="oldNoti"> old dashboard notice </param>
        /// <param name="newContents"> new contents </param>
        void UpdateDashboardNotice(Notification oldNoti, string newContents);

        /// <summary>
        /// Delete dashboard notice
        /// </summary>
        /// <param name="oldNoti"> notification to be deleted </param>
        void DeleteDashboardNotice(Notification oldNoti);
    }

    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private static readonly global::System.Globalization.CultureInfo culture;

        public NotificationService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<NotificationService>();
        }

        /// <summary>
        /// Send notification
        /// </summary>
        public void SendMessage(string messageType, string notificationType, string msgUser, string msgBody, int companyId, bool keep = false, string relatedUrl = "")
        {
            try
            {
                var notification = new NotificationProtocolDataDetail
                {
                    MessageType = messageType,
                    NotificationType = notificationType,
                    User = msgUser,
                    Message = msgBody,
                    RelatedUrl = relatedUrl,
                    Keep = keep
                };
                ApplicationVariables.SendMessageToAllClients(Helpers.JsonConvertCamelCase(new SDKDataWebhookModel()
                {
                    Type = Constants.Protocol.Notification,
                    Data = notification
                }), companyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendMessage");
            }
        }

        /// <summary>
        /// Pushing notification
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="content">content message</param>
        /// <param name="receiveId">account id received</param>
        /// <param name="companyId">company id</param>
        public void PushingNotificationToUser(string type, int id, string title, string content, int receiveId, int companyId)
        {
            try
            {
                var companyCode = _unitOfWork.AppDbContext.Company.Find(companyId).Code;
                var messaging = FirebaseMessaging.DefaultInstance;
                var topic = $"{companyCode}_{receiveId}";

                try
                {
                    var notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = title,
                        Body = content,
                    };

                    if (notification != null)
                    {
                        var message = new Message()
                        {
                            Notification = notification,
                            Topic = topic,
                            Data = new Dictionary<string, string>()
                            {
                                { "type", type},
                                { "id", id.ToString() }
                            }
                        };
                        messaging.SendAsync(message);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PushingNotificationToUser");
            }
        }

        public void PushingNotificationToUsers(string type, string title, string content, string companyCode, List<int> receiveIds)
        {
            var messaging = FirebaseMessaging.DefaultInstance;

            foreach (var receiveId in receiveIds)
            {
                var topic = $"{companyCode}_{receiveId}";

                var message = new Message()
                {
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = title,
                        Body = content,
                    },
                    Topic = topic,
                    Data = new Dictionary<string, string>()
                    {
                        { "type", type}
                    }
                };
                messaging.SendAsync(message);
            }
        }

        private void SendNotificationToFe(string type, string content, string receiveId, int companyId)
        {
            var notification = new NotificationVisitorProtocolDataDetail
            {
                Type = type,
                Content = content,
                ReceiveId = receiveId,
                CreatedOn = DateTime.UtcNow.ConvertDefaultDateTimeToString(),
                CompanyId = companyId.ToString(),
                Status = "false"
            };
            ApplicationVariables.SendMessageToAllClients(Helpers.JsonConvertCamelCase(new SDKDataWebhookModel()
            {
                Type = Constants.Protocol.Notification,
                Data = notification
            }), companyId);
        }

        public List<NotificationData> GetNotifications(int companyId, int userId, string sortColumn, string sortDirection, string filter, int pageNumber, int pageSize,
            out int totalRecords, out int recordsFiltered, string firstAccessTime, string lastAccessTime, out int totalUnread)
        {
            try
            {
                //string companyLanguage = "";
                //if (companyId == 0)
                //    companyLanguage = "en-US";
                //else
                //    companyLanguage = new SettingService(_unitOfWork, _configuration).GetLanguage(Convert.ToInt32(companyId));

                //var culture = new CultureInfo(companyLanguage);
                //var culture = new System.Globalization.CultureInfo(0);

                var lstNoti = _unitOfWork.NotificationRepository.GetNotifications(companyId, userId, sortColumn, sortDirection, filter, pageNumber, pageSize,
                out totalRecords, out recordsFiltered, firstAccessTime, lastAccessTime, out totalUnread, culture).ToList();

                return lstNoti;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetNotifications");
                totalRecords = 0;
                recordsFiltered = 0;
                totalUnread = 0;
                return new List<NotificationData>();
            }
        }

        public Notification GetNotificationById(int id)
        {
            var noti = _unitOfWork.NotificationRepository.GetNotificationById(id);
            return noti;
        }


        public Notification MappingNoti(int companyId, int receiveId, int notificationType, string resourceName, string resourceParam, string relatedUrl)
        {
            try
            {
                string companyLanguage = "";
                if (companyId == 0)
                    companyLanguage = "en-US";
                else
                    companyLanguage = new SettingService(_unitOfWork, _configuration).GetLanguage(Convert.ToInt32(companyId));

                string content = "";
                var culture = new CultureInfo(companyLanguage);
                if (notificationType == (short)NotificationType.NotificationAction || notificationType == (short)NotificationType.NotificationVisit)
                {
                    content = string.Format(VisitResource.ResourceManager.GetString(resourceName, culture),
                        JsonConvert.DeserializeObject<NotificationMapping>(resourceParam).visitor_name, JsonConvert.DeserializeObject<NotificationMapping>(resourceParam).visit_date);
                }
                else if (notificationType == (short)NotificationType.NotificationInform || notificationType == (short)NotificationType.NotificationVisit)
                {
                    content = string.Format(VisitResource.ResourceManager.GetString(resourceName, culture),
                        JsonConvert.DeserializeObject<NotificationMapping>(resourceParam).visitor_name, JsonConvert.DeserializeObject<NotificationMapping>(resourceParam).status);
                }
                else if (notificationType == (short)NotificationType.NotificationEmergency)
                {
                    if (resourceName == "NotificationEmergency")
                    {
                        content = string.Format(MailContentResource.ResourceManager.GetString(resourceName, culture),
                        JsonConvert.DeserializeObject<NotificationMapping>(resourceParam).Command,
                        JsonConvert.DeserializeObject<NotificationMapping>(resourceParam).IcuName,
                        JsonConvert.DeserializeObject<NotificationMapping>(resourceParam).building,
                        JsonConvert.DeserializeObject<NotificationMapping>(resourceParam).userOpen);
                    }
                    else if (resourceName == "ReleaseNotification")
                    {
                        content = string.Format(MailContentResource.ResourceManager.GetString(resourceName, culture),
                        JsonConvert.DeserializeObject<NotificationMapping>(resourceParam).buildingName,
                        JsonConvert.DeserializeObject<NotificationMapping>(resourceParam).userRelease);
                    }
                }
                else if (notificationType == (short)NotificationType.NotificationWarning || notificationType == (short)NotificationType.NotificationDoors)
                {
                    content = string.Format(MailContentResource.ResourceManager.GetString(resourceName, culture),
                    JsonConvert.DeserializeObject<NotificationMapping>(resourceParam).IcuName, JsonConvert.DeserializeObject<NotificationMapping>(resourceParam).DeviceAddress,
                    JsonConvert.DeserializeObject<NotificationMapping>(resourceParam).building);


                }

                Notification noti = new Notification();
                noti.Type = notificationType;
                noti.CompanyId = companyId;
                noti.ResourceName = resourceName;
                noti.ResourceParam = resourceParam;
                noti.ReceiveId = receiveId;
                noti.RelatedUrl = relatedUrl;
                noti.Content = content;
                return noti;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MappingNoti");
                return null;
            }
        }

        public ResponseStatus AddNotification(Notification model, int typeNotification, IModel channel = null)
        {
            try
            {
                var res = _unitOfWork.NotificationRepository.AddNotification(model);

                if (res.statusCode)
                {
                    string type = Constants.Protocol.NotificationInform;
                    if (model.Type == (short)NotificationType.NotificationAction)
                        type = Constants.Protocol.NotificationAction;
                    else if (model.Type == (short)NotificationType.NotificationInform)
                        type = Constants.Protocol.NotificationInform;
                    else if (model.Type == (short)NotificationType.NotificationEmergency)
                        type = Constants.Protocol.NotificationWarning;
                    else if (model.Type == (short)NotificationType.NotificationWarning)
                        type = Constants.Protocol.NotificationWarning;

                    SendNotificationToFe(type, model.Content, model.ReceiveId.ToString(), model.CompanyId);
                }

                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddNotification");
                return new ResponseStatus { statusCode = false };
            }
        }


        public ResponseStatus UpdateStatus(int Id, NotificationUpdate model)
        {
            var res = _unitOfWork.NotificationRepository.UpdateStatus(Id, model);

            return res;
        }

        public ResponseStatus DeleteNotification(int id)
        {

            var res = _unitOfWork.NotificationRepository.DeleteNotification(id);

            return res;
        }

        public ResponseStatus UpdateMultipleStatus(List<int> lstStatus, bool Status)
        {

            var res = _unitOfWork.NotificationRepository.UpdateMultipleStatus(lstStatus, Status);

            return res;
        }

        public ResponseStatus DeleteMultipleStatus(List<int> lstId)
        {

            var res = _unitOfWork.NotificationRepository.DeleteMultipleStatus(lstId);

            return res;
        }
        public ResponseStatus DeleteAllStatus(int companyId, int userId)
        {

            var res = _unitOfWork.NotificationRepository.DeleteAllStatus(companyId, userId);

            return res;
        }
        public ResponseStatus UnReadAll(int companyId, int userId)
        {
            var res = _unitOfWork.NotificationRepository.UnReadAll(companyId, userId);

            return res;
        }

        public bool CreateNotificationToUsers(NotificationNoticeModel model, int companyId)
        {
            bool result = true;
            IEnumerable<int> accountIds;
            if (model.UserIds == null || !model.UserIds.Any())
            {
                accountIds = _unitOfWork.UserRepository.GetUserByCompany(companyId)
                    .Where(m => m.AccountId.HasValue)
                    .Select(m => m.AccountId.Value);

            }
            else
            {
                accountIds = _unitOfWork.UserRepository.GetByIds(companyId, model.UserIds)
                    .Where(m => m.AccountId.HasValue)
                    .Select(m => m.AccountId.Value);
            }

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var accountId in accountIds)
                        {
                            _unitOfWork.AppDbContext.Notification.Add(new Notification()
                            {
                                CompanyId = companyId,
                                Type = (short)NotificationType.NotificationNotice,
                                CreatedOn = DateTime.Now,
                                Status = false,
                                Content = model.Content,
                                ReceiveId = accountId,
                                RelatedUrl = "/notifications",
                                ResourceName = "",
                                ResourceParam = ""
                            });
                            _unitOfWork.Save();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        _logger.LogError(ex.StackTrace);
                        transaction.Rollback();
                        result = false;
                    }
                }
            });

            var companyCode = _unitOfWork.CompanyRepository.GetById(companyId).Code;
            new Thread(() =>
            {
                PushingNotificationToUsers("info", NotificationResource.TitlleNotificationNotice, model.Content, companyCode, accountIds.ToList());
            }).Start();

            return result;
        }

        public NoticeModel GetDashboardNotice(int accountId, int companyId)
        {
            var noticeData = new NoticeModel();

            List<int> visitStatus = new List<int>()
            {
                (short)VisitChangeStatusType.Waiting,
                (short)VisitChangeStatusType.Approved1,
            };
            noticeData.TotalVisitRequest = _unitOfWork.AppDbContext.Visit.Count(m => visitStatus.Contains(m.Status) && m.CompanyId == companyId && !m.IsDeleted);
            List<int> accessStatus = new List<int>()
            {
                (short) ApprovalStatus.ApprovalWaiting1,
                (short) ApprovalStatus.ApprovalWaiting2,
            };
            noticeData.TotalAccessRequest = _unitOfWork.AppDbContext.User.Count(m => accessStatus.Contains(m.ApprovalStatus) && m.CompanyId == companyId && !m.IsDeleted);

            noticeData.Notices = _unitOfWork.NotificationRepository.GetNotifications(companyId, accountId, "CreatedOn", "desc", null, 1, 5,
                out _, out _, null, null, out _, culture).ToList();

            return noticeData;
        }

        /// <summary>
        /// Add Dashboard notification
        /// </summary>
        /// <param name="notice"> contents of notice </param>
        /// <param name="companyId"> company identifier </param>
        /// <returns></returns>
        public ResponseStatus AddDashBoardNotice(string notice, int companyId)
        {
            try
            {
                Notification newNoti = new Notification()
                {
                    CompanyId = companyId,
                    Content = notice,
                    ReceiveId = 0,
                    RelatedUrl = "",
                    ResourceName = "",
                    ResourceParam = "",
                    Type = (int)NotificationType.NotificationNotice
                };

                ResponseStatus result = _unitOfWork.NotificationRepository.AddNotification(newNoti);
                _unitOfWork.Save();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddDashBoardNotice");
                return new ResponseStatus { statusCode = false };
            }
        }

        /// <summary>
        /// Get dashboard notice.
        /// </summary>
        /// <param name="companyId"> company identifier </param>
        /// <returns></returns>
        public NoticeModel GetDashboardNotice(int companyId)
        {
            NoticeModel result = new NoticeModel();

            List<int> visitStatus = new List<int>()
            {
                (short)VisitChangeStatusType.Waiting,
                (short)VisitChangeStatusType.Approved1,
            };
            result.TotalVisitRequest = _unitOfWork.AppDbContext.Visit.Count(m => visitStatus.Contains(m.Status) && m.CompanyId == companyId && !m.IsDeleted);
            List<int> accessStatus = new List<int>()
            {
                (short) ApprovalStatus.ApprovalWaiting1,
                (short) ApprovalStatus.ApprovalWaiting2,
            };
            result.TotalAccessRequest = _unitOfWork.AppDbContext.User.Count(m => accessStatus.Contains(m.ApprovalStatus) && m.CompanyId == companyId && !m.IsDeleted);

            var notices = _unitOfWork.NotificationRepository.GetNotificationByCompanyId(companyId).Where(n => n.ReceiveId == 0).ToList()
                .Select(n => new NotificationData()
                {
                    CompanyId = n.CompanyId,
                    CreatedOn = n.CreatedOn,
                    Content = n.Content,
                    TypeId = n.Type,
                    Type = ((NotificationType) n.Type).GetDescription(),
                    Id = n.Id,
                }).ToList();

            result.Notices = notices;

            return result;
        }

        /// <summary>
        /// Update dashboard notice
        /// </summary>
        /// <param name="oldNoti"></param>
        /// <param name="newContents"></param>
        public void UpdateDashboardNotice(Notification oldNoti, string newContents)
        {
            try
            {
                oldNoti.Content = newContents;

                _unitOfWork.NotificationRepository.Update(oldNoti);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateDashboardNotice");
            }
        }

        /// <summary>
        /// Delete dashboard notice
        /// </summary>
        /// <param name="oldNoti"></param>
        public void DeleteDashboardNotice(Notification oldNoti)
        {
            try
            {
                _unitOfWork.NotificationRepository.Delete(oldNoti);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteDashboardNotice");
            }
        }
    }
}
