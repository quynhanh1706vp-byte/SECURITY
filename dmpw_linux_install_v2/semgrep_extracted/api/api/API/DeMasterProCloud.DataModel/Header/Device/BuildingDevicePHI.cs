using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Infrastructure.Header;

namespace DeMasterProCloud.DataModel.Header
{
    public class BuildingDevicePHI : IPageInfo
    {
        private static readonly string[] headerList;
        private static readonly string pageName;
        private static readonly string pageType;

        static BuildingDevicePHI()
        {
            headerList = new string[] {
                DeviceHeaderColumns.DoorName.GetName(),
                DeviceHeaderColumns.DeviceAddress.GetName(),
                DeviceHeaderColumns.DeviceType.GetName(),
                DeviceHeaderColumns.ActiveTz.GetName(),
                DeviceHeaderColumns.PassageTz.GetName(),
            };

            pageName = Page.Building + Page.Device;
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
