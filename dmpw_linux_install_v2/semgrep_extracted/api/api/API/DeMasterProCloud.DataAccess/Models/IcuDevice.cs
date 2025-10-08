using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeMasterProCloud.DataAccess.Models
{
    public partial class IcuDevice
    {
        public IcuDevice()
        {
            EventLog = new HashSet<EventLog>();
            AccessGroupDevice = new HashSet<AccessGroupDevice>();
            DeviceReader = new HashSet<DeviceReader>();
        }

        public int Id { get; set; }

        public string DeviceAddress { get; set; }
        public string Name { get; set; }

        public short DeviceType { get; set; }
        public short VerifyMode { get; set; }
        public short BioStationMode { get; set; }
        public int BackupPeriod { get; set; } = 3;

        public Company Company { get; set; }
        public int? CompanyId { get; set; }
        public Building Building { get; set; }
        public int? BuildingId { get; set; }
        public AccessTime ActiveTz { get; set; }
        public int? ActiveTzId { get; set; }
        public AccessTime PassageTz { get; set; }
        public int? PassageTzId { get; set; }

        public string IpAddress { get; set; }

        [StringLength(50)]
        public string ServerIp { get; set; }
        public int ServerPort { get; set; }
        public string MacAddress { get; set; }

        public short OperationType { get; set; }

        // 0 = In , 1 = Out
        public short? RoleReader0 { get; set; } = 0;
        public short? RoleReader1 { get; set; } = 1;

        // 0 = Blue, 1 = Red
        public short? LedReader0 { get; set; } = 0;
        public short? LedReader1 { get; set; } = 0;

        // 0 = On, 1 = Off
        public short? BuzzerReader0 { get; set; } = 0;
        public short? BuzzerReader1 { get; set; } = 0;

        // 0 = Use, 1 = Not use
        public int? UseCardReader { get; set; }

        public short SensorType { get; set; }
        public int? OpenDuration { get; set; } = 3;
        public int? MaxOpenDuration { get; set; } // maximum value of open duration door can support
        public int? SensorDuration { get; set; } = 3;
        public bool SensorAlarm { get; set; }
        public bool CloseReverseLockFlag { get; set; }

        // 0 = Not use, 1 = Soft APB, 2 = Hard APB
        public short PassbackRule { get; set; } = 0;

        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }

        public DateTime LastCommunicationTime { get; set; }
        public string CreateTimeOnlineDevice { get; set; }
        public int UpTimeOnlineDevice { get; set; }
        // 0 = Valid, 1 = Invalid
        public short Status { get; set; }
        // 0 = use MPR, 1 = Don't use MPR
        //public int Condition { get; set; }
        public int MPRCount { get; set; }
        //public int TapRange { get; set; }
        public int MPRInterval { get; set; }

        public short ConnectionStatus { get; set; }
        public string DoorStatus { get; set; }

        /// <summary>
        /// Id value of doorStatus
        /// </summary>
        public int DoorStatusId { get; set; }

        public short AlarmStatus { get; set; }

        public string FirmwareVersion { get; set; }
        public string VersionReader0 { get; set; }
        public string VersionReader1 { get; set; }
        public string NfcModuleVersion { get; set; }
        public string ExtraVersion { get; set; }

        public int RegisterIdNumber { get; set; }
        public int EventCount { get; set; }
        public int NumberOfNotTransmittingEvent { get; set; }

        public bool IsDeleted { get; set; }

        /// <summary>   Gets or sets the device buzzer. </summary>
        /// <value> The device buzzer. \n
        ///         This is used for ICU-300N. \n
        ///         0 = OFF, 1 = ON</value>
        public short DeviceBuzzer { get; set; } = 1;
        
        // Id for partner
        public string AliasId { get; set; }
        
        public string Image { get; set; }
        public bool UseAlarmRelay { get; set; }

        // For LPR
        public string DeviceManagerIds { get; set; }
        public string DependentDoors { get; set; }
        public bool AutoAcceptVideoCall { get; set; }
        public bool EnableVideoCall { get; set; }

        public ICollection<EventLog> EventLog { get; set; }
        public ICollection<AccessGroupDevice> AccessGroupDevice { get; set; }
        public ICollection<DepartmentDevice> DepartmentDevice { get; set; }
        public ICollection<DeviceReader> DeviceReader { get; set; }
    }
}
