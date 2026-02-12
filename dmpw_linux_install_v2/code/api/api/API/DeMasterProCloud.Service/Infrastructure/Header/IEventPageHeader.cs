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
    public interface IEventPageHeader : IGeneralPageHeader
    {
        //List<HeaderData> GetHeaderList(PlugIn plugIn);
    }

    public class EventPageHeader : IEventPageHeader
    {
        //private readonly IUnitOfWork _unitOfWork;

        private readonly string _pageName;
        private readonly string[] _headerNameList;

        public EventPageHeader(/*IUnitOfWork unitOfWork, */IPageInfo pageInfo)
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
                object item = Array.Find((EventsHeaderColumns[])Enum.GetValues(typeof(EventsHeaderColumns)), m => m.GetName().Equals(column));

                // Check vehicle plugin
                if ((int)item == (int)EventsHeaderColumns.VehicleImage && !plugIns.VehiclePlugIn)
                {
                    continue;
                }

                if (item != null && column.Equals(item.GetName()))
                {
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



        //public List<HeaderData> GetUserHeaderList(string pageName, int companyId, int accountId)
        //{
        //    var plugIn = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyId);
        //    var strPlugIns = plugIn.PlugIns;
        //    var plugIns = JsonConvert.DeserializeObject<PlugIns>(strPlugIns);

        //    var headerList = pageHeader.UserPageHeader.GetHeaderList(plugIn);

        //    return pageHeader.CheckHeaderSetting(headerList, companyId, accountId);
        //}
    }
}
