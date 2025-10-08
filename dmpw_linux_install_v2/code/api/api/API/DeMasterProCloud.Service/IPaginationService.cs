using System.Linq;

namespace DeMasterProCloud.Service
{
    public interface IPaginationService<out T> where T : class
    {
        /// <summary>
        /// Interface support for pagination using datatale jquery
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pageNumber">the index of page which start to load</param>
        /// <param name="sortDirection"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <param name="sortColumn"></param>
        /// <returns></returns>
        IQueryable<T> GetPaginated(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);
    }
}
