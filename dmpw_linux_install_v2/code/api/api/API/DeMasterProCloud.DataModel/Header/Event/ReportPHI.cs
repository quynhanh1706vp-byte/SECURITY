using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Infrastructure.Header;

namespace DeMasterProCloud.DataModel.Header
{
    public class ReportPHI : IPageInfo
    {
        private static readonly string[] headerList;
        private static readonly string pageName;
        private static readonly string pageType;

        static ReportPHI()
        {
            headerList = new string[] {
                EventsHeaderColumns.Id.GetName(),
                EventsHeaderColumns.EventTime.GetName(),
                EventsHeaderColumns.UserName.GetName(),
                EventsHeaderColumns.DepartmentName.GetName(),
                EventsHeaderColumns.CardId.GetName(),
                EventsHeaderColumns.EventDetail.GetName(),
                EventsHeaderColumns.DoorName.GetName(),
                EventsHeaderColumns.Building.GetName(),
                EventsHeaderColumns.InOut.GetName(),

                EventsHeaderColumns.DeviceAddress.GetName(),

                EventsHeaderColumns.IssueCount.GetName(),
                EventsHeaderColumns.CardStatus.GetName(),
                EventsHeaderColumns.CardType.GetName(),
                EventsHeaderColumns.WorkTypeName.GetName(),
                EventsHeaderColumns.Memo.GetName(),
                EventsHeaderColumns.VehicleImage.GetName(),
            };

            pageName = Page.Report;
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
