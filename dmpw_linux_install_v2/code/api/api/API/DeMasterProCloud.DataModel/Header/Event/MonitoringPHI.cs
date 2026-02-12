using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Infrastructure.Header;

namespace DeMasterProCloud.DataModel.Header
{
    public class MonitoringPHI : IPageInfo
    {
        private static readonly string[] headerList;
        private static readonly string pageName;
        private static readonly string pageType;

        static MonitoringPHI()
        {
            headerList = new string[] {
                EventsHeaderColumns.Id.GetName(),
                EventsHeaderColumns.EventTime.GetName(),
                EventsHeaderColumns.CardId.GetName(),
                EventsHeaderColumns.UserName.GetName(),
                EventsHeaderColumns.DepartmentName.GetName(),
                EventsHeaderColumns.WorkTypeName.GetName(),
                EventsHeaderColumns.DoorName.GetName(),
                EventsHeaderColumns.EventDetail.GetName(),
                EventsHeaderColumns.InOut.GetName(),
                EventsHeaderColumns.DeviceAddress.GetName(),
                EventsHeaderColumns.IssueCount.GetName(),
                EventsHeaderColumns.ExpireDate.GetName(),
                EventsHeaderColumns.CardStatus.GetName(),
                EventsHeaderColumns.Memo.GetName(),
            };

            pageName = Page.Monitoring;
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
