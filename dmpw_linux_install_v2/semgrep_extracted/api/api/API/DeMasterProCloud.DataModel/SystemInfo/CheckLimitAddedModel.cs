namespace DeMasterProCloud.DataModel.SystemInfo
{
    public class CheckLimitAddedModel
    {
        public int NumberOfMaximum { get; set; }
        public int NumberOfCurrent { get; set; }
        public bool IsAdded { get; set; }
        //
        // public CheckLimitAddedModel()
        // {
        //     
        // }
        //
        // public CheckLimitAddedModel(int current, int maximum)
        // {
        //     NumberOfCurrent = current;
        //     NumberOfMaximum = maximum;
        //     
        // }
    }
}