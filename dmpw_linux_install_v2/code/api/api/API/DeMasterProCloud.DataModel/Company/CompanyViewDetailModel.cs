using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Company
{
    public class CompanyViewDetailModel
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public int NumberOfBuildings { get; set; }
        public int NumberOfDevices { get; set; }
        public int NumberOfUsers { get; set; }
        public int NumberOfAccounts { get; set; }
        public List<string> EnabledPlugIns { get; set; }
        public List<GraphModel> TotalVisitors { get; set; }
        public List<GraphModel> TotalEventLogs { get; set; }
    }

    public class GraphModel
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}