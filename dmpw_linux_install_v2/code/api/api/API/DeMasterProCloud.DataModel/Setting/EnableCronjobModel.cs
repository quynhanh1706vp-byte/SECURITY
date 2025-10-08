namespace DeMasterProCloud.DataModel.Setting
{
    public class EnableCronjobModel
    {
        public bool UpdateUptimePerADay { get; set; } = true;
        public bool CheckNotifyOverDueBook{ get; set; } = true;
        public bool AutoDeleteNotification{ get; set; } = true;
        public bool BackupFileToS3{ get; set; } = true;
        public bool CheckLimitStoredFileMedia{ get; set; } = true;
        public bool RecheckAttendanceCheckin{ get; set; } = true;
        public bool CheckImageEventLogCameraHanet{ get; set; } = true;
        public bool SendDeviceCommonInstruction{ get; set; } = true;
        public bool CheckVisitorExpired{ get; set; } = true;
        public bool CheckMeetingSendToDevice{ get; set; } = true;
        public bool CreateAttendanceNewDay{ get; set; } = true;
        public bool BackupEventLog{ get; set; } = true;
        public bool BackupSystemLog{ get; set; } = true;
        public bool CheckHFaceIdSyncBetweenDmpAndHanet{ get; set; } = true;
    }
}