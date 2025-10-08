using System.Collections.Generic;

namespace DeMasterProCloud.Service.Protocol
{
    /// <summary>
    /// Update meal service time to device
    /// </summary>
    public class UpdateMealServiceTimeProtocolData : ProtocolData<UpdateMealServiceTimeProtocolHeaderData>
    {
    }

    public class UpdateMealServiceTimeProtocolHeaderData
    {
        public UpdateMealServiceTimeProtocolHeaderData()
        {
            MealServiceTime = new List<MealServiceTimeDetail>();
        }
        public int Total { get; set; }
        public List<MealServiceTimeDetail> MealServiceTime { get; set; }
    }

    public class MealServiceTimeDetail
    {
        public string weekday { get; set; }
        public string mealService { get; set; }
        public int eventType { get; set; }
        public string message { get; set; }
        public string interval { get; set; }
        public string corner { get; set; }
        public int cornerId { get; set; }
    }

}
