using DeMasterProCloud.DataAccess.Models;

namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    public class SortColumnMapping
    {
        public static string UserColumn(string sortColumn)
        {
            if (string.IsNullOrWhiteSpace(sortColumn))
            {
                return "FirstName";
            }

            sortColumn = char.ToUpper(sortColumn[0]) + sortColumn.Substring(1);
            switch (sortColumn)
            {
                case "DepartmentName": return "Department.DepartName";
                case "EmployeeNo": return "EmpNumber";
                case "AccessGroupName": return "AccessGroup.Name";
                default: return typeof(User).GetProperty(sortColumn) == null ? "FirstName" : sortColumn;
            }
        }
        
        public static string DeviceColumn(string sortColumn)
        {
            if (string.IsNullOrWhiteSpace(sortColumn))
            {
                return "Name";
            }
            
            sortColumn = char.ToUpper(sortColumn[0]) + sortColumn.Substring(1);
            switch (sortColumn)
            {
                case "DoorName": return "Name";
                case "FwVersion": return "FirmwareVersion";
                case "DoorActiveTimeZone": return "ActiveTz.Name";
                case "DoorPassageTimeZone": return "PassageTz.Name";
                case "Building": return "Building.Name";
                case "companyName": return "Company.Name";
                default: return typeof(IcuDevice).GetProperty(sortColumn) == null ? "Name" : sortColumn;
            }
        }
        
        public static string VisitColumn(string sortColumn)
        {
            if (string.IsNullOrWhiteSpace(sortColumn))
            {
                return "Status";
            }
            
            sortColumn = char.ToUpper(sortColumn[0]) + sortColumn.Substring(1);
            switch (sortColumn)
            {
                case "ProcessStatus": return "Status";
                default: return typeof(Visit).GetProperty(sortColumn) == null ? "Status" : sortColumn;
            }
        }
        
        public static string VisitReportColumn(string sortColumn)
        {
            if (string.IsNullOrWhiteSpace(sortColumn))
            {
                return "EventTime";
            }
            
            sortColumn = char.ToUpper(sortColumn[0]) + sortColumn.Substring(1);
            switch (sortColumn)
            {
                case "AccessTime": return "EventTime";
                case "Avatar": return "ResultCheckIn";
                case "InOut": return "Antipass";
                case "Building": return "Icu.Building.Name";
                case "BirthDay": return "Visit.BirthDay";
                case "Device": return "Icu.DeviceAddress";
                case "EventDetail": return "EventType";
                default: return typeof(EventLog).GetProperty(sortColumn) == null ? "EventTime" : sortColumn;
            }
        }

        public static string EventLogReportColumn(string sortColumn)
        {
            if (string.IsNullOrWhiteSpace(sortColumn))
            {
                return "EventTime";
            }
            
            sortColumn = char.ToUpper(sortColumn[0]) + sortColumn.Substring(1);
            switch (sortColumn)
            {
                case "AccessTime": return "EventTime";
                case "BirthDay": return "User.BirthDay";
                case "UserCode": return "User.UserCode";
                case "Department": return "User.Department.DepartName";
                case "DeviceAddress": return "Icu.DeviceAddress";
                case "EventDetail": return "EventType";
                case "Building": return "Icu.Building.Name";
                case "InOut": return "Antipass";
                default: return typeof(EventLog).GetProperty(sortColumn) == null ? "EventTime" : sortColumn;
            }
        }
        
        public static string SystemLogColumn(string sortColumn)
        {
            if (string.IsNullOrWhiteSpace(sortColumn))
            {
                return "OpeTime";
            }

            sortColumn = char.ToUpper(sortColumn[0]) + sortColumn.Substring(1);
            switch (sortColumn)
            {
                case "OperationTime": return "OpeTime";
                case "UserAccount": return "CreatedByNavigation.Username";
                case "OperationType": return "Type";
                case "Action": return "Action";
                case "Details": return "ContentDetails";
                default: return typeof(SystemLog).GetProperty(sortColumn) == null ? "OpeTime" : sortColumn;
            }
        }

        public static string AccessibleDoorColumn(string sortColumn)
        {
            if (string.IsNullOrWhiteSpace(sortColumn))
            {
                return "Name";
            }

            sortColumn = char.ToUpper(sortColumn[0]) + sortColumn.Substring(1);
            switch (sortColumn)
            {
                case "DeviceAddress": return "DeviceAddress";
                case "ActiveTz": return "ActiveTz.Name";
                case "PassageTz": return "PassageTz.Name";
                case "VerifyMode": return "VerifyMode";
                case "AntiPassback": return "PassbackRule";
                case "DeviceType": return "DeviceType";
                case "mpr": return "MPRCount";
                default: return typeof(IcuDevice).GetProperty(sortColumn) == null ? "Name" : sortColumn;
            }
        }

        public static string FirmwareVersionColumn(string sortColumn)
        {
            if (string.IsNullOrWhiteSpace(sortColumn))
            {
                return "Version";
            }

            sortColumn = char.ToUpper(sortColumn[0]) + sortColumn.Substring(1);
            switch (sortColumn)
            {
                default: return typeof(FirmwareVersion).GetProperty(sortColumn) == null ? "Version" : sortColumn;
            }
        }

        public static string DepartmentColumn(string sortColumn)
        {
            if (string.IsNullOrEmpty(sortColumn))
            {
                return "DepartName";
            }
            
            sortColumn = char.ToUpper(sortColumn[0]) + sortColumn.Substring(1);
            switch (sortColumn)
            {
                case "DepartmentName": return "DepartName";
                case "DepartmentNumber": return "DepartNo";
                case "DepartmentManager": return "DepartmentManager.Username";
                default: return typeof(Department).GetProperty(sortColumn) == null ? "DepartName" : sortColumn;
            }
        }
    }
}