using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Infrastructure.Header;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Header;
using DeMasterProCloud.DataModel.PlugIn;
using DeMasterProCloud.Repository;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeMasterProCloud.Service.Infrastructure.Header
{
    public interface IUserPageHeader : IGeneralPageHeader
    {

    }

    public class UserPageHeader : IUserPageHeader
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly string _pageName;
        private readonly string[] _headerNameList;

        public UserPageHeader(IUnitOfWork unitOfWork, IPageInfo pageInfo)
        {
            // Set UnitOfWork.
            _unitOfWork = unitOfWork;
            // Set pageName.
            _pageName = pageInfo.GetPageName();
            // Set list of header name.
            _headerNameList = pageInfo.GetHeaderNameList();
        }

        public List<HeaderData> GetHeaderList(PlugIn plugIn)
        {
            // Create default header data
            List<HeaderData> headers = new List<HeaderData>();
            // Check companyId. If the companyId is 0, return default header data.
            if (plugIn == null || plugIn.CompanyId == 0) { return headers; }
            // Check plugins.
            var strPlugIn = plugIn.PlugIns;
            var plugIns = JsonConvert.DeserializeObject<PlugIns>(strPlugIn);
            // Order is started from 0.
            int headerOrder = 0;

            foreach (var column in _headerNameList)
            {
                object item = Array.Find((UserHeaderColumns[])Enum.GetValues(typeof(UserHeaderColumns)), m => m.GetName().Equals(column));

                if (item != null && column.Equals(item.GetName()))
                {
                    // Check vehicle plugin
                    if ((int)item == (int)UserHeaderColumns.PlateNumberList && !plugIns.VehiclePlugIn)
                    {
                        continue;
                    }

                    // Check army plugin (MilitaryNo / EmployeeNo)
                    if ((int)item == (int)UserHeaderColumns.MilitaryNo)
                    {
                        continue;
                    }

                    // Check Access Approval
                    if ((int)item == (int)UserHeaderColumns.ApprovalStatus)
                    {
                        var accessSetting = _unitOfWork.AppDbContext.AccessSetting.Where(m => m.CompanyId == plugIn.CompanyId).FirstOrDefault();
                        // don't show approval status when not using approval setting
                        if (accessSetting.ApprovalStepNumber == (short)VisitSettingType.NoStep)
                            continue;
                    }

                    HeaderData header = new HeaderData()
                    {
                        PageName = _pageName,
                        HeaderId = (int)item,
                        HeaderOrder = headerOrder++,
                        HeaderName = item.GetDescription(),
                        HeaderVariable = item.GetName().Replace("_Army", ""),
                        IsCategory = false
                    };

                    headers.Add(header);
                }
            }

            return headers;
        }
    }
}
