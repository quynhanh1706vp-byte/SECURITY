//using Microsoft.AspNetCore.Http;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;

//namespace DeMasterProCloud.Common.Infrastructure
//{
//    /// <summary>
//    /// for Permission class
//    /// </summary>
//    public class Permission
//    {
//        public static Dictionary<string, Dictionary<string, bool>> GetPermssions(string plugInName)
//        {
//            switch (plugInName)
//            {
//                case Constants.PlugIn.Common:
//                    return GetCommonPermssions();
//                case Constants.PlugIn.AccessControl:
//                    return GetAccessControlPermssions();
//                case Constants.PlugIn.TimeAttendance:
//                    return GetTimeAttendencePermssions();
//                case Constants.PlugIn.CanteenManagement:
//                    return GetCanteenPermssions();
//                case Constants.PlugIn.VisitManagement:
//                    return GetVisitPermssions();
//                case Constants.PlugIn.ScreenMessage:
//                    return GetDeviceMessagePermssions();
//                case Constants.PlugIn.ArmyManagement:
//                    return GetArmyPermissions();
//                default:
//                    return new Dictionary<string, Dictionary<string, bool>>();
//            }
//        }

//        /// <summary>
//        /// Get permission list about common plugIn
//        /// </summary>
//        /// <returns></returns>
//        public static Dictionary<string, Dictionary<string, bool>> GetCommonPermssions()
//        {
//            Dictionary<string, Dictionary<string, bool>> permissionList = new Dictionary<string, Dictionary<string, bool>>();

//            #region Add permission to Dictionary

//            // Add account permission
//            permissionList.Add(PermissionGroupName.Account, new AccountPermission().GetPermissions());

//            // Add building permission
//            permissionList.Add(PermissionGroupName.Building, new BuildingPermission().GetPermissions());

//            // Add department permission
//            permissionList.Add(PermissionGroupName.Department, new DepartmentPermission().GetPermissions());

//            // Add user permission
//            permissionList.Add(PermissionGroupName.User, new UserPermission().GetPermissions());

//            // Add device permission
//            permissionList.Add(PermissionGroupName.Device, new DevicePermission().GetPermissions());

//            // Add eventLog permission
//            permissionList.Add(PermissionGroupName.EventLog, new EventLogPermission().GetPermissions());

//            // Add systemLog permission
//            permissionList.Add(PermissionGroupName.SystemLog, new SystemLogPermission().GetPermissions());

//            // Add setting permission
//            permissionList.Add(PermissionGroupName.Setting, new SettingPermission().GetPermissions());

//            #endregion

//            return permissionList;
//        }

//        /// <summary>
//        /// Get permission list about access control plugIn
//        /// </summary>
//        /// <returns></returns>
//        public static Dictionary<string, Dictionary<string, bool>> GetAccessControlPermssions()
//        {
//            Dictionary<string, Dictionary<string, bool>> permissionList = new Dictionary<string, Dictionary<string, bool>>();

//            #region Add permission to Dictionary

//            // Add access group permission
//            permissionList.Add(PermissionGroupName.AccessGroup, new AccessGroupPermission().GetPermissions());

//            // Add category permission
//            permissionList.Add(PermissionGroupName.Category, new CategoryPermission().GetPermissions());

//            // Add holiday permission
//            permissionList.Add(PermissionGroupName.Holiday, new HolidayPermission().GetPermissions());

//            // Add timezone permission
//            permissionList.Add(PermissionGroupName.Timezone, new TimezonePermission().GetPermissions());

//            #endregion

//            return permissionList;
//        }

//        /// <summary>
//        /// Get permission list about time attendance plugIn
//        /// </summary>
//        /// <returns></returns>
//        public static Dictionary<string, Dictionary<string, bool>> GetTimeAttendencePermssions()
//        {
//            Dictionary<string, Dictionary<string, bool>> permissionList = new Dictionary<string, Dictionary<string, bool>>();

//            #region Add permission to Dictionary
//            // Add attendance permission
//            permissionList.Add(PermissionGroupName.Attendance, new AttendancePermission().GetPermissions());

//            // Add workingType permission
//            permissionList.Add(PermissionGroupName.WorkingType, new WorkingTypePermission().GetPermissions());
//            #endregion

//            return permissionList;
//        }

//        /// <summary>
//        /// Get permission list about canteen plugIn
//        /// </summary>
//        /// <returns></returns>
//        public static Dictionary<string, Dictionary<string, bool>> GetCanteenPermssions()
//        {
//            Dictionary<string, Dictionary<string, bool>> permissionList = new Dictionary<string, Dictionary<string, bool>>();

//            #region Add permission to Dictionary
//            // Add corner setting permission
//            permissionList.Add(PermissionGroupName.CornerSetting, new CornerSettingPermission().GetPermissions());

//            // Add exceptional meal permission
//            permissionList.Add(PermissionGroupName.ExceptionalMeal, new ExceptionalMealPermission().GetPermissions());

//            // Add meal eventLog permission
//            permissionList.Add(PermissionGroupName.MealEventLog, new MealEventLogPermission().GetPermissions());

//            //Add meal setting permission
//            permissionList.Add(PermissionGroupName.MealSetting, new MealSettingPermission().GetPermissions());

//            // Add meal type meal permission
//            permissionList.Add(PermissionGroupName.MealType, new MealTypePermission().GetPermissions());
            
//            #endregion

//            return permissionList;
//        }

//        /// <summary>
//        /// Get permission list about visit plugIn
//        /// </summary>
//        /// <returns></returns>
//        public static Dictionary<string, Dictionary<string, bool>> GetVisitPermssions()
//        {
//            Dictionary<string, Dictionary<string, bool>> permissionList = new Dictionary<string, Dictionary<string, bool>>();

//            #region Add permission to Dictionary
//            // Add Visit permission
//            permissionList.Add(PermissionGroupName.Visit, new VisitPermission().GetPermissions());
            
//            #endregion

//            return permissionList;
//        }

//        /// <summary>
//        /// Get permission list about device message plugIn
//        /// </summary>
//        /// <returns></returns>
//        public static Dictionary<string, Dictionary<string, bool>> GetDeviceMessagePermssions()
//        {
//            Dictionary<string, Dictionary<string, bool>> permissionList = new Dictionary<string, Dictionary<string, bool>>();

//            #region Add permission to Dictionary
//            // Add Device Message permission
//            permissionList.Add(PermissionGroupName.DeviceMessage, new DeviceMessagePermission().GetPermissions());

//            #endregion

//            return permissionList;
//        }

//        /// <summary>
//        /// Get permission list about army management plugIn
//        /// </summary>
//        /// <returns></returns>
//        public static Dictionary<string,Dictionary<string, bool>> GetArmyPermissions()
//        {
//            Dictionary<string, Dictionary<string, bool>> permissionList = new Dictionary<string, Dictionary<string, bool>>();

//            #region Add permission to Dictionary
//            // Add army user permission
//            permissionList.Add(PermissionGroupName.UserArmy, new UserArmyPermission().GetPermissions());

//            // Add army visitor permission
//            permissionList.Add(PermissionGroupName.VisitArmy, new VisitArmyPermission().GetPermissions());

//            #endregion

//            return permissionList;
//        }
        
//    }

//    // Dynamic Role/Permission
//    // Maybe these are used to store to DB as JSON form or get data from DB.
//    public class PermissionGroupName
//    {
//        public const string AccessGroup = "AccessGroup";
//        public const string Account = "Account";
//        public const string Building = "Building";
//        public const string Category = "Category";
//        public const string Department = "Department";
//        public const string Device = "Device";
//        public const string EventLog = "EventLog";
//        public const string Holiday = "Holiday";
//        public const string Setting = "Setting";
//        public const string SystemLog = "SystemLog";
//        public const string Timezone = "Timezone";
//        public const string User = "User";

//        public const string Visit = "Visit";

//        public const string CornerSetting = "CornerSetting";
//        public const string ExceptionalMeal = "ExceptionalMeal";
//        public const string MealEventLog = "MealEventLog";
//        public const string MealSetting = "MealSetting";
//        public const string MealType = "MealType";
//        public const string UserDiscount = "UserDiscount";

//        public const string DeviceMessage = "DeviceMessage";

//        public const string Attendance = "Attendance";
//        public const string WorkingType = "WorkingType";

//        public const string UserArmy = "UserArmy";
//        public const string VisitArmy = "VisitArmy";
//    }

//    public class PermissionActionName
//    {
//        public const string View = "View";
//        public const string Add = "Add";
//        public const string Edit = "Edit";
//        public const string Delete = "Delete";

//        public const string AssignDoor = "AssignDoor";

//        public const string Export = "Export";

//        public const string SendInstruction = "SendInstruction";
//        public const string ViewHistory = "ViewHistory";
//        public const string Reinstall = "Reinstall";
//        public const string Reboot = "Reboot";
//        public const string Copy = "Copy";

//        public const string ReturnCard = "ReturnCard";
//    }

//    //public interface PermissionClass
//    //{
//    //    Dictionary<string, bool> GetPermissions();
//    //}
//    public class PermissionClass
//    {
//        public const bool View = true;
//        public const bool Add = false;
//        public const bool Edit = false;
//        public const bool Delete = false;

//        public Dictionary<string, bool> GetPermissions()
//        {
//            var className = this.GetType().Name.Replace("Permission", "");
//            Dictionary<string, bool> permissions = new Dictionary<string, bool>();

//            foreach (FieldInfo fi in typeof(PermissionClass).GetFields())
//            {
//                permissions.Add(fi.Name + className, Convert.ToBoolean(fi.GetValue((object)fi.Name).ToString()));
//            }

//            foreach (FieldInfo fi in GetType().GetFields())
//            {
//                if(permissions.ContainsKey(fi.Name + className))
//                    permissions.Remove(fi.Name + className);

//                permissions.Add(fi.Name + className, Convert.ToBoolean(fi.GetValue((object)fi.Name).ToString()));
//            }
            
//            return permissions;
//        }
//    }

//    public class AccessGroupPermission : PermissionClass
//    {
//        public const bool AssignDoor = false;
//    }

//    public class AccountPermission : PermissionClass
//    {
//    }

//    public class AttendancePermission : PermissionClass
//    {
//        public const bool Export = false;
//    }

//    public class BuildingPermission : PermissionClass
//    {
//        public const bool AssignDoor = false;
//    }

//    public class CategoryPermission : PermissionClass{}

//    public class CornerSettingPermission : PermissionClass{}

//    public class DepartmentPermission : PermissionClass
//    {
//        public const bool Export = false;
//    }

//    public class DevicePermission : PermissionClass
//    {
//        public const bool SendInstruction = false;
//        public const bool ViewHistory = false;
//        public const bool Reinstall = false;
//        public const bool Reboot = false;
//        public const bool Copy = false;
//    }

//    public class DeviceMessagePermission : PermissionClass{}

//    public class EventLogPermission : PermissionClass{}

//    public class ExceptionalMealPermission : PermissionClass{}

//    public class HolidayPermission : PermissionClass{}

//    public class MealEventLogPermission : PermissionClass{}

//    public class MealSettingPermission : PermissionClass{}

//    public class MealTypePermission : PermissionClass{}

//    public class SettingPermission : PermissionClass{}

//    public class SystemLogPermission : PermissionClass
//    {
//        public const bool Export = false;
//    }

//    public class TimezonePermission : PermissionClass{}

//    public class UserPermission : PermissionClass{}

//    public class VisitPermission : PermissionClass
//    {
//        public const bool Export = false;
//        public const bool ReturnCard = false;
//        public const bool ViewHistory = false;

//        public const bool AssignDoor = false;
//    }

//    public class WorkingTypePermission : PermissionClass{}

//    public class UserArmyPermission : PermissionClass{}

//    public class VisitArmyPermission : PermissionClass{}
//}
