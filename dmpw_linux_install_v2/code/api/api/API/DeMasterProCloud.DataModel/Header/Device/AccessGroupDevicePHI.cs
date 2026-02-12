using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Infrastructure.Header;

namespace DeMasterProCloud.DataModel.Header
{
    public class AccessGroupDevicePHI : IPageInfo
    {
        private static readonly string[] headerList;
        private static readonly string pageName;
        private static readonly string pageType;

        static AccessGroupDevicePHI()
        {
            headerList = new string[] {
                DeviceHeaderColumns.DeviceAddress.GetName(),
                DeviceHeaderColumns.DoorName.GetName(),
                DeviceHeaderColumns.AccessTime.GetName(),
                DeviceHeaderColumns.Building.GetName(),
            };

            pageName = Page.AccessGroup + Page.Device;
            pageType = Page.Device;
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
