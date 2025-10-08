using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace DeMasterProCloud.DataAccess.Models
{
    public class Company
    {
        public Company()
        {
            Account = new HashSet<Account>();
            AccessGroup = new HashSet<AccessGroup>();
            Department = new HashSet<Department>();
            EventLog = new HashSet<EventLog>();
            Holiday = new HashSet<Holiday>();
            IcuDevice = new HashSet<IcuDevice>();
            SystemLog = new HashSet<SystemLog>();
            AccessTime = new HashSet<AccessTime>();
            User = new HashSet<User>();
            Building = new List<Building>();
            Visit = new HashSet<Visit>();
            Card = new HashSet<Card>();
            
            DynamicRole = new HashSet<DynamicRole>();
            CompanyAccount = new HashSet<CompanyAccount>();
            Face = new HashSet<Face>();

            ApprovalHistory = new HashSet<ApprovalHistory>();

            HeaderSetting = new HashSet<HeaderSetting>();

            // For Data List setting (ex. Header)
            DataListSetting = new HashSet<DataListSetting>();

            AccessSchedule = new HashSet<AccessSchedule>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Contact { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ExpiredFrom { get; set; }
        public DateTime ExpiredTo { get; set; }
        public byte[] Logo { get; set; }
        public byte[] MiniLogo { get; set; }
        public string Name { get; set; } = "";
        public string Remarks { get; set; }
        public bool RootFlag { get; set; }
        public bool AutoSyncUserData { get; set; }
        public int EventLogStorageDurationInDb { get; set; }
        public int EventLogStorageDurationInFile { get; set; }
        public int TimeLimitStoredImage { get; set; } // days
        public int TimeLimitStoredVideo { get; set; } // days
        public bool UpdateAttendanceRealTime { get; set; }
        public int TimeRecheckAttendance { get; set; }
        public int LimitCountOfUser { get; set; }
        public bool EnableReCheckImageCamera { get; set; }
        public int TimeLimitCheckImageCamera { get; set; } // hours

        /// <summary>
        /// This variable is for card Id length fixation settings.
        /// </summary>
        /// <value>
        /// 0 : Normal
        /// 64 : 64bits -> 8bytes ( ex. 11223344(X) -> 1122334455667788(O) )
        /// </value>
        public int CardBit { get; set; }

        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }

        public string SecretCode { get; set; }

        /// <summary>
        /// This variable is for the option whether to use valid period of password.
        /// </summary>
        public bool UseExpiredPW { get; set; }

        /// <summary>
        /// This variable means that the valid period (days) of password.
        /// </summary>
        public int PwValidPeriod { get; set; }

        /// <summary>
        /// This variable is for the option whether to use encryption of DB data.
        /// </summary>
        public bool UseDataEncrypt { get; set; }

        public string WebsiteUrl { get; set; }
        public string ContactWEmail { get; set; }
        public string Phone { get; set; }
        public string Industries { get; set; }
        public string Location { get; set; }

        public ICollection<Account> Account { get; set; }
        public ICollection<AccessGroup> AccessGroup { get; set; }
        public ICollection<Department> Department { get; set; }
        public ICollection<EventLog> EventLog { get; set; }
        public ICollection<Holiday> Holiday { get; set; }
        public ICollection<IcuDevice> IcuDevice { get; set; }
        public ICollection<SystemLog> SystemLog { get; set; }
        public ICollection<AccessTime> AccessTime { get; set; }
        public ICollection<User> User { get; set; }
        public ICollection<UnregistedDevice> UnregistedDevice { get; set; }
        public ICollection<Building> Building { get; set; }
        public ICollection<Visit> Visit { get; set; }
        public ICollection<Card> Card { get; set; }
        public ICollection<Face> Face { get; set; }
        
        public ICollection<WorkingType> WorkingType { get; set; }
        
        public ICollection<PlugIn> PlugIn { get; set; }
        
        public VisitSetting VisitSetting { get; set; }
        public AccessSetting AccessSetting { get; set; }
        public AttendanceSetting AttendanceSetting { get; set; }
        public LeaveRequestSetting LeaveRequestSetting { get; set; }
        public ICollection<VisitHistory> VisitHistory { get; set; }
        public DateTime UpdatedAttendanceOn { get; set; }
        public ICollection<DynamicRole> DynamicRole { get; set; }
        public ICollection<CompanyAccount> CompanyAccount { get; set; }

        public ICollection<ApprovalHistory> ApprovalHistory { get; set; }

        public ICollection<HeaderSetting> HeaderSetting { get; set; }
        
        /// <summary>
        /// For data list setting (ex. Header)
        /// </summary>
        public ICollection<DataListSetting> DataListSetting { get; set; }

        public ICollection<AccessSchedule> AccessSchedule { get; set; }
    }
}
