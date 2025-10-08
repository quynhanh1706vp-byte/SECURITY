using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Device
{
    public class CameraModel
    {
        public string Name { get; set; }
        public int IcuId { get; set; }
        public int CompanyId { get; set; }
        public int VideoLength { get; set; }
        public string CameraId { get; set; }
        public int Type { get; set; }
        // public bool AutoOpenDoor { get; set; }
        // public int TimeOpenDoor { get; set; }
        public int RoleReader { get; set; }
        public bool SaveEventUnknownFace { get; set; }
        public bool CheckEventFromWebHook { get; set; }
        public string UrlStream { get; set; }
        public int Similarity { get; set; } 
        public bool VoiceAlarm { get; set; } 
        public bool LightAlarm { get; set; }
    }

    public class CameraListModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PlaceID { get; set; }
        public int IcuId { get; set; }
        public int VideoLength { get; set; }
        public int CompanyId { get; set; }
        public string DeviceName { get; set; }
        public string CameraId { get; set; }
        public short ConnectionStatus { get; set; }
        public int Type { get; set; }
        // public bool AutoOpenDoor { get; set; }
        // public int TimeOpenDoor { get; set; }
        public int RoleReader { get; set; }
        public bool SaveEventUnknownFace { get; set; }
        public bool CheckEventFromWebHook { get; set; }
        public int? BuildingId { get; set; }
        public string UrlStream { get; set; }
        public string VmsUrlStream { get; set; }
        public int Similarity { get; set; } 
        public bool VoiceAlarm { get; set; } 
        public bool LightAlarm { get; set; }
    }

    public class CameraDeviceConfig
    {
        public string CameraId { get; set; }
        public int RoleReader { get; set; }
        public bool SaveEventUnknownFace { get; set; }
        public int Similarity { get; set; } 
        public bool VoiceAlarm { get; set; } 
        public bool LightAlarm { get; set; }
    }

    public class DataImageCamera
    {
        public string FileName { get; set; }
        public string Link { get; set; }
        public string UserName { get; set; }
        public int? UserId { get; set; }
    }

    public class AvatarModel
    {
        public string Avatar { get; set; }
    }

    #region model integrated api camera hanet

    public class HanetPlaceInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    
    public class HanetCameraInfo
    {
        public string Address { get; set; }
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public string PlaceName { get; set; }
    }
    
    public class CameraEventCheckInModel
    {
        public string PersonName { get; set; }
        public string Date { get; set; }
        public long CheckinTime { get; set; }
        public string PlaceID { get; set; }
        public string PersonID { get; set; }
        public string Avatar { get; set; }
        public string Place { get; set; }
        public string Type { get; set; }
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
    }

    public class CameraPersonModel
    {
        public string Name { get; set; }
        public string PlaceID { get; set; }
        public string PersonID { get; set; }
        public string Title { get; set; }
        public int Type { get; set; }
        public string Avatar { get; set; }
        public string UrlAvatar { get; set; }
        public int UserId { get; set; }
        public int VisitId { get; set; }
    }
    public class CameraDeparmentModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NumEmployee { get; set; }
        // public string PlaceId { get; set; }
        // public int Status { get; set; }
        // public string Desc { get; set; }
        // public int Enable { get; set; }
        // public string CreatedAt { get; set; }
        // public string UpdatedAt { get; set; }
        public List<CameraPersonModel> Persons { get; set; } = new List<CameraPersonModel>();
        
    }

    public class CheckinDataWebhookModel
    {
        public string Action_type { get; set; }
        public string Data_type { get; set; }
        public string Date { get; set; }
        public string Detected_image_url { get; set; }
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public string Hash { get; set; }
        public string Id { get; set; }
        public string Keycode { get; set; }
        public string PersonID { get; set; }
        public string PersonName { get; set; }
        public string PersonType { get; set; }
        public string PlaceID { get; set; }
        public string PlaceName { get; set; }
        public long Time { get; set; }
        public double Temp { get; set; }
    }

    public class HanetUserDataWebhookModel
    {
        public string Action_type { get; set; }
        public string Data_type { get; set; }
        public string Date { get; set; }
        public string Avatar { get; set; }
        public string Hash { get; set; }
        public string Id { get; set; }
        public string Keycode { get; set; }
        public string PersonID { get; set; }
        public string PersonName { get; set; }
        public string PersonType { get; set; }
        public string PlaceID { get; set; }
        public string PlaceName { get; set; }
        public long Time { get; set; }
    }

    public class QrCodeDataWebhookModel
    {
        public string Qrcode { get; set; }
        public string Sign { get; set; }
        public string DeviceID { get; set; }
        public long Timestamp { get; set; }
    }

    #endregion
}