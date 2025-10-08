using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Infrastructure.Header;

namespace DeMasterProCloud.DataModel.Header
{
    public class AccessibleDoorDevicePHI : IPageInfo
    {
        private static readonly string[] headerList;
        private static readonly string pageName;
        private static readonly string pageType;

        static AccessibleDoorDevicePHI()
        {
            headerList = new string[] {
                DeviceHeaderColumns.Id.GetName(),
                DeviceHeaderColumns.DoorName.GetName(),
                DeviceHeaderColumns.DeviceAddress.GetName(),
                DeviceHeaderColumns.ActiveTz.GetName(),
                DeviceHeaderColumns.PassageTz.GetName(),
                DeviceHeaderColumns.VerifyMode.GetName(),
                DeviceHeaderColumns.AntipassBack.GetName(),
                DeviceHeaderColumns.DeviceType.GetName(),
                DeviceHeaderColumns.Mpr.GetName(),
                DeviceHeaderColumns.Action.GetName(),
            };

            pageName = Page.AccessibleDoor + Page.Device;
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
