using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Infrastructure.Header;

namespace DeMasterProCloud.DataModel.Header
{
    public class UserPHI : IPageInfo
    {
        private static readonly string[] headerList;
        private static readonly string pageName;
        private static readonly string pageType;

        static UserPHI()
        {
            headerList = new string[] {
                UserHeaderColumns.Id.GetName(),
                UserHeaderColumns.FirstName.GetName(),
                UserHeaderColumns.UserCode.GetName(),
                UserHeaderColumns.DepartmentName.GetName(),
                UserHeaderColumns.Position.GetName(),
                UserHeaderColumns.EmployeeNo.GetName(),
                UserHeaderColumns.ExpiredDate.GetName(),
                UserHeaderColumns.WorkTypeName.GetName(),
                UserHeaderColumns.CardList.GetName(),
                UserHeaderColumns.FaceList.GetName(),
                UserHeaderColumns.PlateNumberList.GetName(),
                UserHeaderColumns.ApprovalStatus.GetName(),
                UserHeaderColumns.Action.GetName(),
            };

            pageName = Page.User;
            pageType = Page.User;
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
