using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Infrastructure.Header;

namespace DeMasterProCloud.DataModel.Header
{
    public class VehicleMonitoringPHI : IPageInfo
    {
        private static readonly string[] headerList;
        private static readonly string pageName;
        private static readonly string pageType;

        static VehicleMonitoringPHI()
        {
            headerList = new string[] {
                EventsHeaderColumns.Id.GetName(),
                EventsHeaderColumns.EventTime.GetName(),
                EventsHeaderColumns.DoorName.GetName(),
                EventsHeaderColumns.Model.GetName(),
                EventsHeaderColumns.PlateNumber.GetName(),
                EventsHeaderColumns.DepartmentName.GetName(),
                EventsHeaderColumns.UserName.GetName(),
                EventsHeaderColumns.EventDetail.GetName(),
                EventsHeaderColumns.InOut.GetName(),
                EventsHeaderColumns.VehicleImage.GetName(),
                EventsHeaderColumns.VehicleType.GetName(),
            };

            pageName = Page.VehicleManagement + Page.Monitoring;
            pageType = Page.Event;
        }

        /// <summary>
        /// Get default header list
        /// </summary>
        /// <returns></returns>
        public string[] GetHeaderNameList()
        {
            return headerList;
        }

        /// <summary>
        /// Get page name value.
        /// </summary>
        /// <returns></returns>
        public string GetPageName(){
            return pageName;
        }

        /// <summary>
        /// Get page type value.
        /// </summary>
        /// <returns></returns>
        public string GetPageType()
        {
            return pageType;
        }
    }
}
