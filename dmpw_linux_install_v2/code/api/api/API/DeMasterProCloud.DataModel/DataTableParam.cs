using DeMasterProCloud.Common.Infrastructure;

namespace DeMasterProCloud.DataModel
{
    public class DataTableParam
    {
        public int? Start { get; set; }
        public string SearchValue { get; set; }
        public int? OrderColumn { get; set; }
        public string OrderDir { get; set; }


        public static DataTableParam Create()
        {
            return new DataTableParam
            {
                OrderColumn = 1,
                Start = 0,
                SearchValue = string.Empty,
                OrderDir = Constants.Pagination.DefaultOrder
            };
        }
    }
}
