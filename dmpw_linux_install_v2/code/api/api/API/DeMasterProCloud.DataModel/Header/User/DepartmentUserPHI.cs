using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Infrastructure.Header;

namespace DeMasterProCloud.DataModel.Header
{
    public class DepartmentUserPHI : IPageInfo
    {
        private static readonly string[] headerList;
        private static readonly string pageName;
        private static readonly string pageType;

        static DepartmentUserPHI()
        {
            headerList = new string[] {
                UserHeaderColumns.Id.GetName(),
                UserHeaderColumns.FirstName.GetName(),
                UserHeaderColumns.Position.GetName(),
                UserHeaderColumns.EmployeeNo.GetName(),
                UserHeaderColumns.ExpiredDate.GetName(),
                UserHeaderColumns.WorkTypeName.GetName(),
                UserHeaderColumns.CardList.GetName(),
                UserHeaderColumns.FaceList.GetName(),
                UserHeaderColumns.PlateNumberList.GetName(),
                UserHeaderColumns.Action.GetName(),
            };

            pageName = Page.Department + Page.User;
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
