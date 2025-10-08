namespace DeMasterProCloud.DataModel
{
    public class FilterModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }

        public FilterModel()
        {
        }
        
        public FilterModel(string sortColumn, string sortDirection)
        {
            PageNumber = 1;
            PageSize = 10;
            SortColumn = sortColumn;
            SortDirection = sortDirection;
        }
        
        public FilterModel(int pageNumber, int pageSize, string sortColumn, string sortDirection)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            SortColumn = sortColumn;
            SortDirection = sortDirection;
        }
    }
}