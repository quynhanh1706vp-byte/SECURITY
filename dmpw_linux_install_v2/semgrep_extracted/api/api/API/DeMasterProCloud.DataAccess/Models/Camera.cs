using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace DeMasterProCloud.DataAccess.Models
{
    public class Camera
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public int PlaceID { get; set; }
        public int? IcuDeviceId { get; set; }
        public int CompanyId { get; set; }
        public int VideoLength { get; set; }
        public short ConnectionStatus { get; set; }
        public DateTime LastCommunicationTime { get; set; }
        public int Type { get; set; }
        // public int TimeOpenDoor { get; set; }
        // public bool AutoOpenDoor { get; set; }
        public bool SaveEventUnknownFace { get; set; }
        public bool SaveEventCommunication { get; set; } // 
        public bool CheckEventFromWebHook { get; set; } // open door when event receive from webhook camera
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CameraId { get; set; }
        public int RoleReader { get; set; }
        public int Similarity { get; set; } 
        public bool VoiceAlarm { get; set; } 
        public bool LightAlarm { get; set; }
        public string UrlStream { get; set; }
        public string VmsUrlStream { get; set; }

        public IcuDevice IcuDevice { get; set; }
        public Company Company { get; set; }
        public ICollection<EventLog> EventLog { get; set; }
    }
}