using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Infrastructure.Header;

namespace DeMasterProCloud.DataModel.Header
{
    public class SystemLogPHI : IPageInfo
    {
        private static readonly string[] headerList;
        private static readonly string pageName;
        private static readonly string pageType;

        static SystemLogPHI()
        {
            headerList = new string[] {
                SystemLogHeaderColumns.OperationTime.GetName(),
                SystemLogHeaderColumns.UserAccount.GetName(),
                SystemLogHeaderColumns.OperationType.GetName(),
                SystemLogHeaderColumns.OperationAction.GetName(),
                SystemLogHeaderColumns.Message.GetName(),
                SystemLogHeaderColumns.Details.GetName()
            };

            pageName = Page.SystemLog;
            pageType = Page.SystemLog;
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
