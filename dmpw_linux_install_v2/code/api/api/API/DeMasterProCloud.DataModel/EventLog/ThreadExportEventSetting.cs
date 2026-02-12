namespace DeMasterProCloud.DataModel.EventLog
{
    public class ThreadExportEventSetting
    {
        public int MaxPageSize { get; set; }
        public int MaxRecordInSheet { get; set; }
        public int MaximumRecordExport { get; set; }
        public int AllowMaximumRecordExport { get; set; }

        // For hancell
        public int MaximumRecordExportHancell { get; set; }
        public int AllowMaximumRecordExportHancell { get; set; }
    }
}