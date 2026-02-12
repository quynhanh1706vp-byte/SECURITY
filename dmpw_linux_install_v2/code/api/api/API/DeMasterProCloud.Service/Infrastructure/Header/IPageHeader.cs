using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Header;
using DeMasterProCloud.DataModel.PlugIn;
using DeMasterProCloud.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMasterProCloud.Service.Infrastructure.Header
{
    public interface IPageHeader
    {
        List<HeaderData> GetHeaderList(int companyId, int accountId);
    }


    public class PageHeader : IPageHeader
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly PlugIn _plugIn;

        private IGeneralPageHeader _generalPageHeader;

        private IPageInfo _pageInfo;

        public PageHeader(IConfiguration configuration, string pageName, int companyId)
        {
            _configuration = configuration;
            _unitOfWork = DbHelper.CreateUnitOfWork(configuration);

            if (companyId != 0)
            {
                _plugIn = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyId);
                _pageInfo = MatchPageInfo(pageName);
                SetHeaderFactory();
            }
        }

        public IPageInfo MatchPageInfo(string pageName)
        {
            switch (pageName)
            {
                //=============== For user pages ===============
                case Page.User:
                    return new UserPHI();

                case Page.AccessGroup + Page.User:
                    return new AccessGroupUserPHI();

                case Page.AccessGroup + Page.UnAssignUser:
                    return new AccessGroupUnassignUserPHI();

                case Page.AccessibleDoor + Page.User:
                    return new AccessibleDoorUserPHI();

                case Page.RegisteredUser:
                    return new RegisteredUserPHI();

                case Page.Building + Page.User:
                case Page.BuildingMaster:
                    return new BuildingMasterPHI();

                case Page.Building + Page.UnAssignUser:
                case Page.UnassignBuildingMaster:
                    return new BuildingUnassignMasterPHI();

                case Page.Department + Page.User:
                    return new DepartmentUserPHI();

                case Page.Department + Page.UnAssignUser:
                    return new DepartmentUnassignUserPHI();

                case Page.VisitSetting + Page.User:
                    return new VisitSettingAccountPHI();

                case Page.AccessSetting + Page.User:
                    return new AccessSettingAccountPHI();

                case Page.VisitManagement + Page.User:
                    return new VisitUserPHI();
                //===============================================


                //=============== For event pages ===============
                case Page.Monitoring:
                    return new MonitoringPHI();

                case Page.Report:
                    return new ReportPHI();

                case Page.VehicleManagement + Page.Monitoring:
                    return new VehicleMonitoringPHI();
                //===============================================


                //=============== For device pages ===============
                case Page.DeviceSetting:
                    return new DeviceSettingPHI();

                case Page.DeviceMonitoring:
                    return new DeviceMonitoringPHI();

                case Page.Building + Page.Device:
                    return new BuildingDevicePHI();

                case Page.Building + Page.UnassignDevice:
                    return new BuildingUnassignDevicePHI();

                case Page.AccessGroup + Page.Device:
                    return new AccessGroupDevicePHI();

                case Page.AccessGroup + Page.UnassignDevice:
                    return new AccessGroupUnassignDevicePHI();

                case Page.AccessibleDoor + Page.Device:
                    return new AccessibleDoorDevicePHI();
                //===============================================


                //=============== For event pages ===============
                case Page.SystemLog:
                    return new SystemLogPHI();
                //===============================================


                //=============== Default =======================
                default:
                    return null;
            }
        }


        /// <summary>
        /// Set the header factory by pageName.
        /// </summary>
        public void SetHeaderFactory()
        {
            var pageType = _pageInfo.GetPageType();
            switch (pageType)
            {
                //=============== For user pages ================
                case Page.User:
                    _generalPageHeader ??= new UserPageHeader(_unitOfWork, _pageInfo);
                    break;
                //===============================================


                //=============== For event pages ===============
                case Page.Event:
                    _generalPageHeader ??= new EventPageHeader(_pageInfo);
                    break;
                //===============================================


                //=============== For device pages ===============
                case Page.Device:
                    _generalPageHeader ??= new DevicePageHeader(_pageInfo);
                    break;
                //===============================================


                //=============== For systemLog pages ===============
                case Page.SystemLog:
                    _generalPageHeader ??= new SystemLogPageHeader(_pageInfo);
                    break;
                //===============================================


                //=============== Default =======================
                default:
                    _generalPageHeader ??= new GeneralPageHeader();
                    break;
                //===============================================
            }
        }

        public List<HeaderData> GetHeaderList(int companyId, int accountId)
        {
            try
            {
                if (companyId == 0)
                    return [];

                //var plugIn = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyId);
                List<HeaderData> headers = _generalPageHeader.GetHeaderList(_plugIn);

                if (headers == null || headers.Count == 0) return headers;

                List<string> categoryPage =
                [
                    //=============== For event pages ===============
                    Page.VehicleManagement + Page.Monitoring,
                    //===============================================
                    
                    //=============== For user pages ================
                    Page.User,

                    Page.RegisteredUser,
                    Page.AccessibleDoor,

                    Page.VisitManagement + Page.User,
                    Page.VisitSetting + Page.User,

                    Page.AccessGroup + Page.User,
                    Page.AccessGroup + Page.UnAssignUser,

                    Page.Building + Page.User,
                    Page.BuildingMaster,
                    Page.Building + Page.UnAssignUser,
                    Page.UnassignBuildingMaster,

                    Page.Department + Page.User,
                    Page.Department + Page.UnAssignUser,
                    //===============================================
                ];

                var headerSetting = _unitOfWork.DataListSettingRepository.GetHeaderByCompanyAndAccount(companyId, accountId);
                if (headerSetting == null)
                {
                    return headers;
                }

                if (string.IsNullOrEmpty(headerSetting.DataList))
                {
                    return headers;
                }

                string pageName = headers.First().PageName;

                var headerListInDb = headerSetting.DataList;
                var headerSettingModels = JsonConvert.DeserializeObject<List<HeaderSettingModel>>(headerListInDb);
                var pageHeaders = headerSettingModels.FirstOrDefault(m => m.PageName == pageName);

                if (pageHeaders == null)
                {
                    return headers;
                }

                var headerInfoList = pageHeaders.Headers;

                foreach (var headerInfo in headerInfoList)
                {
                    var header = headers.FirstOrDefault(m => m.HeaderVariable == headerInfo.HeaderVariable);

                    if (header != null)
                    {
                        header.HeaderOrder = headerInfo.HeaderOrder;
                        header.IsVisible = headerInfo.IsVisible;
                    }
                }

                headers = headers.OrderBy(m => m.HeaderOrder).ToList();

                return headers;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] Header Setting : {e.Message}");
                Console.WriteLine($"[ERROR] Header Setting : {e.StackTrace}");
                Console.WriteLine($"[ERROR] Header Setting : {e.InnerException?.Message}");

                return [];
            }
        }
    }
}
