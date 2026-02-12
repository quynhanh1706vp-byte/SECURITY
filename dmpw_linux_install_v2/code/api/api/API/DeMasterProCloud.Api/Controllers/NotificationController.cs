using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.Notification;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// Contorller of Notification
    /// </summary>
    [Produces("application/json")]
    // [Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class NotificationController : ControllerBase
    {
        
        INotificationService _notificationService;
        private readonly HttpContext _httpContext;

        /// <summary>
        /// controller of notification
        /// </summary>
        public NotificationController(INotificationService notificationService, IHttpContextAccessor contextAccessor)
        {
            _notificationService = notificationService;
            _httpContext = contextAccessor.HttpContext;

        }

        /// <summary>
        /// Get list of notification by user (include companyId and userId)
        /// </summary>
        /// <param name="search">Query string that filter notifications by content</param>
        /// <param name="pageNumber">Page number start from 1</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <param name="firstAccessTime">String of first access time</param>
        /// <param name="lastAccessTime">String of last access time</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiNotification)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetListNotification(string sortColumn = "Type", string sortDirection = "desc", string search = null, int pageNumber = 1, int pageSize = 10, string firstAccessTime = null, string lastAccessTime = null)
        {
            sortColumn = Helpers.CheckPropertyInObject<NotificationData>(sortColumn, "Type", ColumnDefines.Notification);
            var lstNotifications = _notificationService.GetNotifications(_httpContext.User.GetCompanyId(), _httpContext.User.GetAccountId(), sortColumn, sortDirection, search, pageNumber, pageSize, out var recordsTotal,
                out var recordsFiltered, firstAccessTime, lastAccessTime, out var totalUnread);

            var pagingData = new PagingData<NotificationData>
            {
                Data = lstNotifications,

                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered, TotalUnRead = totalUnread }
            };
            return Ok(pagingData);
        }


        /// <summary>
        /// Get company notices
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiCompanyNotices)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetCompanyNotices(string search, int pageNumber = 1, int pageSize = 5)
        {
            var listCompanyNotices = _notificationService.GetNotifications(_httpContext.User.GetCompanyId(), 0, null, null, search, pageNumber, pageSize, out int recordsTotal, out int recordsFiltered, null, null, out int totalUnread);
            var pagingData = new PagingData<NotificationData>
            {
                Data = listCompanyNotices,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered}
            };
            return Ok(pagingData);
        }


        /// <summary>
        /// Add company notice
        /// </summary>
        /// <param name="notice"> notice contents </param>
        /// <returns></returns>
        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiCompanyNotices)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult AddDashboardNotice(string notice)
        {
            if (string.IsNullOrWhiteSpace(notice))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, String.Format(MessageResource.Required, NotificationResource.TitlleNotificationNotice));
            }

            var result = _notificationService.AddDashBoardNotice(notice, _httpContext.User.GetCompanyId());

            if (result.statusCode)
                return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageAddSuccess, NotificationResource.TitlleNotificationNotice));
            else
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(MessageResource.MessageAddNewFailed, NotificationResource.TitlleNotificationNotice));
        }


        /// <summary>
        /// Update company notice
        /// </summary>
        /// <param name="id"> notice identifier </param>
        /// <param name="notice"> new notice contents </param>
        /// <returns></returns>
        [HttpPut]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiCompanyNoticesId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult UpdateDashboardNotice(int id, string notice)
        {
            if (string.IsNullOrWhiteSpace(notice))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, String.Format(MessageResource.Required, NotificationResource.TitlleNotificationNotice));
            }

            var dashboardNotice = _notificationService.GetNotificationById(id);
            if (dashboardNotice == null)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.NoData);
            }

            _notificationService.UpdateDashboardNotice(dashboardNotice, notice);

            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageUpdateSuccess, NotificationResource.TitlleNotificationNotice, ""));
        }

        /// <summary>
        /// Delete company notice
        /// </summary>
        /// <param name="id"> notice identifier </param>
        /// <returns></returns>
        [HttpDelete]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiCompanyNoticesId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult DeleteDashboardNotice(int id)
        {
            var dashboardNotice = _notificationService.GetNotificationById(id);
            if (dashboardNotice == null)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.NoData);
            }

            _notificationService.DeleteDashboardNotice(dashboardNotice);

            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageDeleteSuccess, NotificationResource.TitlleNotificationNotice));
        }


        /// <summary>
        /// Get notification by id value
        /// </summary>
        /// <param name="id">Notification id</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUpdateNotification)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetNotificationById(int id)
        {
            var noti = _notificationService.GetNotificationById(id);
            return Ok(noti);
        }

        /// <summary>
        /// Update status
        /// </summary>
        /// <param name="id">Notification id</param>
        /// <param name="model">JSON model include status boolean read or unread</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUpdateNotification)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult UpdateStatus(int id, [FromBody] NotificationUpdate model)
        {
            var responseStatus = _notificationService.UpdateStatus(id, model);
            return Ok(responseStatus);
        }

        /// <summary>
        /// Delete notification by id
        /// </summary>
        /// <param name="id">notification id</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUpdateNotification)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult DeleteNotification(int id)
        {
            var responseStatus = _notificationService.DeleteNotification(id);
            return Ok(responseStatus);
        }

        /// <summary>
        /// Update multiple status
        /// </summary>
        /// <param name="lstStatus">list of notification ids to update status</param>
        /// <param name="Status">boolean status read or unread</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUpdateMultipleNotification)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult UpdateMultipleStatus([FromBody] List<int> lstStatus, bool Status)
        {
            var responseStatus = _notificationService.UpdateMultipleStatus(lstStatus, Status);
            return Ok(responseStatus);
        }

        /// <summary>
        /// Delete Multiple status
        /// </summary>
        /// <param name="lstStatus">list of notification ids</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDeleteMultipleNotification)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult DeleteMultipleStatus([FromBody] List<int> lstStatus)
        {
            var responseStatus = _notificationService.DeleteMultipleStatus(lstStatus);
            return Ok(responseStatus);
        }

        /// <summary>
        /// Delete All status
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDeleteAllNotification)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult DeleteAllStatus()
        {
            var responseStatus = _notificationService.DeleteAllStatus(_httpContext.User.GetCompanyId(), _httpContext.User.GetAccountId());
            return Ok(responseStatus);
        }

        /// <summary>
        /// Mark all notifications as "UnRead"
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiReadAllNotification)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult UnReadAll()
        {
            var responseStatus = _notificationService.UnReadAll(_httpContext.User.GetCompanyId(), _httpContext.User.GetAccountId());
            return Ok(responseStatus);
        }
        
        /// <summary>
        /// Manager user create notification send to users
        /// </summary>
        /// <param name="model">
        /// If count of list UserIds = 0, send default to all users of company
        /// </param>
        /// <returns></returns>
        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiCreateNotification)]
        [Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult CreateNotice([FromBody] NotificationNoticeModel model)
        {
            int companyId = _httpContext.User.GetCompanyId();
            _notificationService.CreateNotificationToUsers(model, companyId);
            return new ApiSuccessResult(StatusCodes.Status201Created,
                string.Format(MessageResource.MessageAddSuccess, NotificationResource.TitlleNotificationNotice));
        }
    }
}