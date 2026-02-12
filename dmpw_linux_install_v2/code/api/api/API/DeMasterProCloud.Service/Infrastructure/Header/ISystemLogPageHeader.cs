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
    public interface ISystemLogPageHeader : IGeneralPageHeader
    {
        //List<HeaderData> GetHeaderList(PlugIn plugIn);
    }

    public class SystemLogPageHeader : IEventPageHeader
    {
        private readonly string _pageName;
        private readonly string[] _headerNameList;

        public SystemLogPageHeader(IPageInfo pageInfo)
        {
            //// Set UnitOfWork.
            //_unitOfWork = unitOfWork;
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
                object item = Array.Find((SystemLogHeaderColumns[])Enum.GetValues(typeof(SystemLogHeaderColumns)), m => m.GetName().Equals(column));

                if (item != null && column.Equals(item.GetName()))
                {
                    HeaderData header = new HeaderData()
                    {
                        PageName = _pageName,
                        HeaderId = (int)item,
                        HeaderOrder = headerOrder++,
                        HeaderName = item.GetDescription(),
                        HeaderVariable = item.GetName(),
                        IsCategory = false
                    };

                    headers.Add(header);
                }
            }

            return headers;
        }
    }
}
