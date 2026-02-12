using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using Microsoft.Extensions.Logging;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Notification;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;

namespace DeMasterProCloud.Repository
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {

        List<NotificationData> GetNotifications(int companyId, int userId, string sortColumn, string sortDirection, string filter, int pageNumber, int pageSize,
            out int totalRecords, out int recordsFiltered, string firstAccessTime, string lastAccessTime, out int totalUnread, CultureInfo culture);
        ResponseStatus UpdateStatus(int Id, NotificationUpdate model);
        ResponseStatus DeleteNotification(int id);
        ResponseStatus AddNotification(Notification model);
        Notification GetNotificationById(int id);
        ResponseStatus UpdateMultipleStatus(List<int> lstStatus, bool Status);
        ResponseStatus DeleteMultipleStatus(List<int> lstId);
        ResponseStatus DeleteAllStatus(int companyId, int userId);
        ResponseStatus UnReadAll(int companyId, int userId);

        void DeleteOldNotification(int period);

        IQueryable<Notification> GetNotificationByCompanyId(int companyId);
        IQueryable<Notification> GetNotificationByAccountIdAndCompanyId(int accountId, int companyId);

    }
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;
        //private readonly HttpContext _httpContext;
        ResponseStatus res = new ResponseStatus();
        public NotificationRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<NotificationRepository>();
        }

        public List<NotificationData> GetNotifications(int companyId, int userId, string sortColumn, string sortDirection, string filter, int pageNumber, int pageSize,
            out int totalRecords, out int recordsFiltered, string firstAccessTime, string lastAccessTime, out int totalUnread, CultureInfo culture)
        {
            var firsTime = DateTime.Now;
            var lastTime = DateTime.Now;
            if (firstAccessTime != null && lastAccessTime != null)
            {
                firsTime = Convert.ToDateTime(DateTimeHelper.ConvertIsoToDateTime(firstAccessTime).ToString("yyyy-MM-dd hh:mm:ss tt"));
                lastTime = Convert.ToDateTime(DateTimeHelper.ConvertIsoToDateTime(lastAccessTime).ToString("yyyy-MM-dd hh:mm:ss tt"));
            }
            
            var notifications = _dbContext.Notification.Where(x => x.CompanyId == companyId && x.ReceiveId == userId).ToList();
            var lstNoti = notifications.Select(x => new NotificationData
            {
                Id = x.Id,
                Type = ((NotificationType)x.Type).GetDescription(),
                TypeId = x.Type,
                Content = !string.IsNullOrEmpty(x.ResourceName) ? Translate(x.Type, x.ResourceName, x.ResourceParam, culture) : x.Content,
                CreatedOn = x.CreatedOn,
                ReceiveId = x.ReceiveId,
                Status = x.Status,
                CompanyId = x.CompanyId,
                RelatedUrl = x.RelatedUrl,
                TransType = ((NotificationType)x.Type).GetDescription()
            });

            if (filter != null)
            {
                lstNoti = lstNoti.Where(x => x.Content.Contains(filter)).ToList();
            }
            else if (firstAccessTime != null && lastAccessTime != null)
            {
                lstNoti = lstNoti.Where(x => x.CreatedOn >= firsTime && x.CreatedOn <= lastTime).ToList();
            }

            if (!string.IsNullOrEmpty(sortColumn))
            {
                var orderBy = $"{sortColumn} {sortDirection}";
                lstNoti = lstNoti.AsQueryable().OrderBy(orderBy).ToList();
            }
            else
            {
                lstNoti = lstNoti.OrderBy(n => n.CreatedOn).ToList();
            }

            totalRecords = lstNoti.Count();

            recordsFiltered = lstNoti.Count();

            totalUnread = lstNoti.Where(x => x.Status == false).Count();

            lstNoti = lstNoti.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();



            return lstNoti.Where(x => x.Content != "" || x.Content != null).ToList();
        }


        public string Translate(int notificationType, string resourceName, string resourceParam, CultureInfo culture)
        {
            string content = "";
            if (notificationType == (short)NotificationType.NotificationAction)
            {
                content = string.Format(VisitResource.ResourceManager.GetString(resourceName, culture),
                    JsonConvert.DeserializeObject<NotificationMapping>(resourceParam).visitor_name, JsonConvert.DeserializeObject<NotificationMapping>(resourceParam).visit_date);
            }
            else if (notificationType == (short)NotificationType.NotificationInform || notificationType == (short)NotificationType.NotificationVisit)
            {
                var status = "";
                var notifyMapping = JsonConvert.DeserializeObject<NotificationMapping>(resourceParam);
                if (notifyMapping.status == "Approved")
                    status = string.Format(VisitResource.ResourceManager.GetString("lblApproval", culture));
                else if (notifyMapping.status == "Rejected")
                    status = string.Format(VisitResource.ResourceManager.GetString("lblReject", culture));
                else if (!string.IsNullOrEmpty(notifyMapping.visit_date))
                    status = notifyMapping.visit_date;
                content = string.Format(VisitResource.ResourceManager.GetString(resourceName, culture),
                    JsonConvert.DeserializeObject<NotificationMapping>(resourceParam).visitor_name,
                    status);
            }
            else if (notificationType == (short)NotificationType.NotificationEmergency)
            {
                if (resourceName.Contains("NotificationEmergency"))
                {
                    var Type = string.Format(CommonResource.ResourceManager.GetString(JsonConvert.DeserializeObject<NotificationMapping>(resourceParam).Type, culture));
                    content = string.Format(MailContentResource.ResourceManager.GetString(resourceName, culture),
                    Type,
                    JsonConvert.DeserializeObject<NotificationMapping>(resourceParam).IcuName,
                    JsonConvert.DeserializeObject<NotificationMapping>(resourceParam).building,
                    JsonConvert.DeserializeObject<NotificationMapping>(resourceParam).userOpen);
                }
                else if (resourceName.Contains("ReleaseNotification1"))
                {
                    content = string.Format(MailContentResource.ResourceManager.GetString(resourceName, culture),
                    JsonConvert.DeserializeObject<NotificationMapping>(resourceParam).IcuName,
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

            return content;
        }

        public string GetNotificationType(int type, string resourceName)
        {

            string content = "";
            if (type == (short)NotificationType.NotificationAction)
            {
                content = Constants.Protocol.NotificationAction;
            }
            else if (type == (short)NotificationType.NotificationInform)
            {
                content = Constants.Protocol.NotificationInform;
            }
            else if (type == (short)NotificationType.NotificationEmergency)
            {
                if (resourceName.Contains("NotificationEmergency"))
                {
                    content = Constants.Protocol.NotificationWarning;
                }
                else if (resourceName.Contains("ReleaseNotification1"))
                {
                    content = Constants.Protocol.NotificationInform;
                }

            }
            else if (type == (short)NotificationType.NotificationWarning)
            {

                if (resourceName.Contains("SubjectDeviceDisconnected"))
                {
                    content = Constants.Protocol.NotificationWarning;
                }
                else if (resourceName.Contains("SubjectDeviceConnected"))
                {
                    content = Constants.Protocol.NotificationInform;
                }


            }
            return content;
        }

        public string GetTranslateType(int type, string resourceName, CultureInfo culture)
        {

            string content = "";
            if (type == (short)NotificationType.NotificationAction)
            {
                content = string.Format(CommonResource.ResourceManager.GetString("NotificationAction", culture));
            }
            else if (type == (short)NotificationType.NotificationInform)
            {
                content = string.Format(CommonResource.ResourceManager.GetString("NotificationInform", culture));
            }
            else if (type == (short)NotificationType.NotificationEmergency)
            {
                if (resourceName.Contains("NotificationEmergency"))
                {
                    content = string.Format(CommonResource.ResourceManager.GetString("NotificationWarning", culture));
                }
                else if (resourceName.Contains("ReleaseNotification1"))
                {
                    content = string.Format(CommonResource.ResourceManager.GetString("NotificationInform", culture));
                }

            }
            else if (type == (short)NotificationType.NotificationWarning || type == (short)NotificationType.NotificationDoors)
            {

                if (resourceName.Contains("SubjectDeviceDisconnected"))
                {
                    content = string.Format(CommonResource.ResourceManager.GetString("NotificationWarning", culture));
                }
                else if (resourceName.Contains("SubjectDeviceConnected"))
                {
                    content = string.Format(CommonResource.ResourceManager.GetString("NotificationInform", culture));
                }


            }
            return content;
        }

        public Notification GetNotificationById(int id)
        {
            try
            {
                return _dbContext.Notification.Find(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetNotificationById");
            }
            return null;
        }

        public IQueryable<Notification> GetNotificationByCompanyId(int companyId)
        {
            return _dbContext.Notification.Where(n => n.CompanyId == companyId);
        }

        public IQueryable<Notification> GetNotificationByAccountIdAndCompanyId(int accountId, int companyId)
        {
            return _dbContext.Notification.Where(n => n.CompanyId == companyId && n.ReceiveId == accountId);
        }


        public ResponseStatus AddNotification(Notification model)
        {
            try
            {
                Notification noti = new Notification();
                noti.CompanyId = model.CompanyId;
                noti.Type = model.Type;
                noti.CreatedOn = DateTime.Now;
                noti.Status = false;
                noti.ReceiveId = model.ReceiveId;
                noti.ResourceName = model.ResourceName ?? "";
                noti.ResourceParam = model.ResourceParam ?? "";
                noti.RelatedUrl = model.RelatedUrl;
                noti.Content = model.Content;
                AddNoti(noti);
                res.statusCode = true;
                res.message = Constants.Notification.AddSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                res.statusCode = false;
                res.message = Constants.Notification.AddFailed;
            }

            return res;
        }


        public ResponseStatus UpdateStatus(int Id, NotificationUpdate model)
        {
            try
            {
                Notification noti = _dbContext.Notification.Find(Id);
                noti.Status = model.Status;
                UpdateNoti(noti);
                res.statusCode = true;
                res.message = Constants.Notification.UpdateNotiSuccess;
            }
            catch (Exception ex)
            {
                ex.ToString();
                res.statusCode = false;
                res.message = Constants.Notification.UpdateFailed;
            }

            return res;
        }

        public ResponseStatus DeleteNotification(int id)
        {

            try
            {
                var notification = _dbContext.Notification.Find(id);
                Remove(notification);
                res.message = Constants.Notification.DeleteSuccess;
                res.statusCode = true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                res.message = Constants.Notification.DeleteFailed;
                res.statusCode = false;
            }

            return res;
        }

        public ResponseStatus UpdateMultipleStatus(List<int> lstStatus, bool Status)
        {
            try
            {

                _dbContext.Notification.Where(x => lstStatus.Contains(x.Id)).ToList().ForEach(a => a.Status = Status);
                _dbContext.SaveChanges();
                res.message = Constants.Notification.UpdateNotiSuccess;
                res.statusCode = true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                res.message = Constants.Notification.DeleteFailed;
                res.statusCode = false;
            }
            return res;
        }

        public ResponseStatus DeleteMultipleStatus(List<int> lstId)
        {
            try
            {
                IEnumerable<Notification> list = _dbContext.Notification.Where(x => lstId.Contains(x.Id));
                _dbContext.RemoveRange(list);
                _dbContext.SaveChanges();
                res.message = Constants.Notification.DeleteSuccess;
                res.statusCode = true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                res.message = Constants.Notification.DeleteFailed;
                res.statusCode = false;
            }
            return res;
        }


        public ResponseStatus DeleteAllStatus(int companyId, int userId)
        {
            try
            {
                IEnumerable<Notification> list = _dbContext.Notification.Where(x => x.CompanyId == companyId && x.Status == true && x.ReceiveId == userId);
                _dbContext.RemoveRange(list);
                _dbContext.SaveChanges();
                res.message = Constants.Notification.DeleteSuccess;
                res.statusCode = true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                res.message = Constants.Notification.DeleteFailed;
                res.statusCode = false;
            }
            return res;
        }

        public void DeleteOldNotification(int period)
        {
            var currentDate = DateTime.UtcNow;

            var limitDate = DateTime.UtcNow.AddDays(-period);

            var oldNotificationList = _dbContext.Notification.Where(x => x.CreatedOn < limitDate);

            try
            {
                _dbContext.RemoveRange(oldNotificationList);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteOldNotification");
            }
        }

        public ResponseStatus UnReadAll(int companyId, int userId)
        {
            try
            {
                _dbContext.Notification.Where(x => x.Status == false && x.CompanyId == companyId && x.ReceiveId == userId).ToList().ForEach(a => a.Status = true);
                _dbContext.SaveChanges();
                res.message = Constants.Notification.UpdateNotiSuccess;
                res.statusCode = true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                res.message = Constants.Notification.UpdateFailed;
                res.statusCode = false;
            }
            return res;
        }


        public void Remove(Notification model)
        {
            try
            {
                _dbContext.Notification.Remove(model);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Remove");
            }
        }

        public void AddNoti(Notification model)
        {
            try
            {
                _dbContext.Notification.Add(model);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddNoti");
            }
        }

        public void UpdateNoti(Notification model)
        {
            try
            {
                _dbContext.Notification.Update(model);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateNoti");
            }
        }

    }
}
