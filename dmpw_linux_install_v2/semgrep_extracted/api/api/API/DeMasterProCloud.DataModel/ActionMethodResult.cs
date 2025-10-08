namespace DeMasterProCloud.DataModel
{
    public class ActionMethodResult
    {
        public string Message { get; set; }

        public bool IsSuccess { get; set; }
    }

    public class ActionResult
    {
        public string Message { get; set; }

        public bool IsSuccess { get; set; }

        public string Data { get; set; }
    }
}
