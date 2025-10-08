using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Infrastructure.Header;

namespace DeMasterProCloud.DataModel.Header
{
    public class DeviceSettingPHI : IPageInfo
    {
        private static readonly string[] headerList;
        private static readonly string pageName;
        private static readonly string pageType;

        static DeviceSettingPHI()
        {
            // About settings (config) value. Exclude status value.
            // These values can be set or checked on DeviceSetting page.
            headerList = new string[] {
                DeviceHeaderColumns.Id.GetName(),
                // Door (device) name.
                DeviceHeaderColumns.DoorName.GetName(),
                // Reader ID (=RID). ex)000001
                DeviceHeaderColumns.DeviceAddress.GetName(),
                // A type of device. ex)ITouchPopX
                DeviceHeaderColumns.DeviceType.GetName(),
                // Active AccessTime name.
                DeviceHeaderColumns.ActiveTz.GetName(),
                // Passage AccessTime name.
                DeviceHeaderColumns.PassageTz.GetName(),
                // Verify mode.
                DeviceHeaderColumns.VerifyMode.GetName(),
                // Name of the building to which the door(device) belongs
                DeviceHeaderColumns.Building.GetName(),
                // A status of progress.
                // This value is status value. But a transmitting all data function is in DeviceSetting page. So it would better this value is here.
                DeviceHeaderColumns.Progress.GetName(),
                // Actions. ex)transmitting all data.
                DeviceHeaderColumns.Action.GetName(),
            };

            pageName = Page.DeviceSetting;
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
