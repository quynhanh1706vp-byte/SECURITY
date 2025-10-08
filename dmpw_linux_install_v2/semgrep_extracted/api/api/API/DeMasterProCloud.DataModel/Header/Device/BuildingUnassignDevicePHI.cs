using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Infrastructure.Header;

namespace DeMasterProCloud.DataModel.Header
{
    public class BuildingUnassignDevicePHI : IPageInfo
    {
        private static readonly string[] headerList;
        private static readonly string pageName;
        private static readonly string pageType;

        static BuildingUnassignDevicePHI()
        {
            headerList = new string[] {
                DeviceHeaderColumns.Building.GetName(),
                DeviceHeaderColumns.DoorName.GetName(),
                DeviceHeaderColumns.DeviceAddress.GetName(),
                DeviceHeaderColumns.DeviceType.GetName(),
            };

            pageName = Page.Building + Page.UnassignDevice;
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
