using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Infrastructure.Header;

namespace DeMasterProCloud.DataModel.Header
{
    public class DeviceMonitoringPHI : IPageInfo
    {
        private static readonly string[] headerList;
        private static readonly string pageName;
        private static readonly string pageType;

        static DeviceMonitoringPHI()
        {
            headerList = new string[] {
                DeviceHeaderColumns.Id.GetName(),
                DeviceHeaderColumns.DoorName.GetName(),
                DeviceHeaderColumns.DeviceAddress.GetName(),
                DeviceHeaderColumns.VerifyMode.GetName(),
                DeviceHeaderColumns.Building.GetName(),
                DeviceHeaderColumns.ConnectionStatus.GetName(),
                DeviceHeaderColumns.Version.GetName(),
                DeviceHeaderColumns.LastCommunicationTime.GetName(),
                DeviceHeaderColumns.DeviceType.GetName(),
                DeviceHeaderColumns.NumberOfNotTransmittingEvent.GetName(),
                DeviceHeaderColumns.RegisterIdNumber.GetName(),
                DeviceHeaderColumns.DoorStatus.GetName(),
                DeviceHeaderColumns.Action.GetName(),
            };

            pageName = Page.DeviceMonitoring;
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
