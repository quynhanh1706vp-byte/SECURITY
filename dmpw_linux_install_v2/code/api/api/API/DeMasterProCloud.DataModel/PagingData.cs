using System.Collections.Generic;
using DeMasterProCloud.DataModel.Header;
using Newtonsoft.Json;

namespace DeMasterProCloud.DataModel
{
    public class PagingData<T>
    {
        public PagingData()
        {
            Meta = new Meta();
        }
        public List<T> Data { get; set; }
        public Meta Meta { get; set; }
    }

    public class Meta
    {
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public int TotalUnRead { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class PagingData<T1, T2>
    {
        public PagingData()
        {
            Meta = new Meta();
        }
        public List<T1> Data { get; set; }
        public List<T2> Header { get; set; }
        public Meta Meta { get; set; }
    }

    public class PagingDataNewObject<T>
    {
        public PagingDataNewObject()
        {
            Meta = new Meta();
        }
        public List<T> Data { get; set; }
        public Meta Meta { get; set; }
        public object DataInit { get; set; }
    }

    public class PagingDataWithHeader<T> : PagingData<T>
    {
        public List<HeaderData> Header { get; set; }
    }
}
