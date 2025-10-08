using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DeMasterProCloud.Common.Infrastructure
{
    /// <summary>
    /// for Page Permission class
    /// </summary>
    public class PagePermission
    {
        public static Dictionary<string, Dictionary<string, bool>> GetPermssionsByPlugIn(string plugInName)
        {
            switch (plugInName)
            {
                case Constants.PlugIn.Common:
                    return GetCommonPermssions();
                case Constants.PlugIn.AccessControl:
                    return GetAccessControlPermssions();
                case Constants.PlugIn.TimeAttendance:
                    return GetTimeAttendencePermssions();
                //case Constants.PlugIn.CanteenManagement:
                //    return GetCanteenPermssions();
                case Constants.PlugIn.VisitManagement:
                    return GetVisitPermssions();
                case Constants.PlugIn.ScreenMessage:
                    return GetDeviceMessagePermssions();
                case Constants.PlugIn.VehiclePlugIn:
                    return GetVehiclePermssions();
                case Constants.PlugIn.CardIssuing:
                    return GetCardIssuingPermssions();
                case Constants.PlugIn.EventManagement:
                    return GetLibraryManagementPermissions();
                default:
                    return new Dictionary<string, Dictionary<string, bool>>();
            }
        }

        /// <summary>
        /// Get permission list about common plugIn
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, bool>> GetCommonPermssions()
        {
            Dictionary<string, Dictionary<string, bool>> permissionList = new Dictionary<string, Dictionary<string, bool>>();

            #region Add permission to Dictionary

            // Add dashboard monitoring page permission
            permissionList.Add(new DashBoardPage().GetPageName(), new DashBoardPage().GetPermissions());

            // Add monitoring page permission
            permissionList.Add(new MonitoringPage().GetPageName(), new MonitoringPage().GetPermissions());

            // Add device monitoring page permission
            permissionList.Add(new DeviceMonitoringPage().GetPageName(), new DeviceMonitoringPage().GetPermissions());


            // Add department page permission
            permissionList.Add(new DepartmentPage().GetPageName(), new DepartmentPage().GetPermissions());

            // Add user page permission
            permissionList.Add(new UserPage().GetPageName(), new UserPage().GetPermissions());


            // Add device setting page permission
            permissionList.Add(new DeviceSettingPage().GetPageName(), new DeviceSettingPage().GetPermissions());

            // Add building page permission
            permissionList.Add(new BuildingPage().GetPageName(), new BuildingPage().GetPermissions());


            // Add report page permission
            permissionList.Add(new ReportPage().GetPageName(), new ReportPage().GetPermissions());

            // Add systemLog permission
            permissionList.Add(new SystemLogPage().GetPageName(), new SystemLogPage().GetPermissions());

            // Add accessible door page permission
            permissionList.Add(new AccessibleDoorPage().GetPageName(), new AccessibleDoorPage().GetPermissions());

            // Add registered user page permission
            permissionList.Add(new RegisteredUserPage().GetPageName(), new RegisteredUserPage().GetPermissions());

            // Add access schedule page permission
            permissionList.Add(new AccessSchedulePage().GetPageName(), new AccessSchedulePage().GetPermissions());

            // Add device reader page permission
            permissionList.Add(new DeviceReaderPage().GetPageName(), new DeviceReaderPage().GetPermissions());

            // Add setting page permission
            permissionList.Add(new SettingPage().GetPageName(), new SettingPage().GetPermissions());

            #endregion

            return permissionList;
        }

        /// <summary>
        /// Get permission list about access control plugIn
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, bool>> GetAccessControlPermssions()
        {
            Dictionary<string, Dictionary<string, bool>> permissionList = new Dictionary<string, Dictionary<string, bool>>();

            #region Add permission to Dictionary

            // Add access group page permission
            permissionList.Add(new AccessGroupPage().GetPageName(), new AccessGroupPage().GetPermissions());

            // Add timezone page permission
            permissionList.Add(new TimezonePage().GetPageName(), new TimezonePage().GetPermissions());

            // Add holiday page permission
            permissionList.Add(new HolidayPage().GetPageName(), new HolidayPage().GetPermissions());

            // Add AccessSetting page permission
            permissionList.Add(new AccessSettingPage().GetPageName(), new AccessSettingPage().GetPermissions());

            #endregion

            return permissionList;
        }

        /// <summary>
        /// Get permission list about time attendance plugIn
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, bool>> GetTimeAttendencePermssions()
        {
            Dictionary<string, Dictionary<string, bool>> permissionList = new Dictionary<string, Dictionary<string, bool>>();

            #region Add permission to Dictionary
            // Add time attendance report page permission
            permissionList.Add(new TimeAttendanceReportPage().GetPageName(), new TimeAttendanceReportPage().GetPermissions());

            // Add working time type page permission
            permissionList.Add(new WorkingTimePage().GetPageName(), new WorkingTimePage().GetPermissions());

            // Add leave request page permission
            permissionList.Add(new LeaveRequestPage().GetPageName(), new LeaveRequestPage().GetPermissions());
            #endregion

            return permissionList;
        }

        /// <summary>
        /// Get permission list about canteen plugIn
        /// </summary>
        /// <returns></returns>
        //public static Dictionary<string, Dictionary<string, bool>> GetCanteenPermssions()
        //{
        //    Dictionary<string, Dictionary<string, bool>> permissionList = new Dictionary<string, Dictionary<string, bool>>();

        //    #region Add permission to Dictionary
        //    // Add corner setting permission
        //    permissionList.Add(PermissionGroupName.CornerSetting, new CornerSettingPermission().GetPermissions());

        //    // Add exceptional meal permission
        //    permissionList.Add(PermissionGroupName.ExceptionalMeal, new ExceptionalMealPermission().GetPermissions());

        //    // Add meal eventLog permission
        //    permissionList.Add(PermissionGroupName.MealEventLog, new MealEventLogPermission().GetPermissions());

        //    //Add meal setting permission
        //    permissionList.Add(PermissionGroupName.MealSetting, new MealSettingPermission().GetPermissions());

        //    // Add meal type meal permission
        //    permissionList.Add(PermissionGroupName.MealType, new MealTypePermission().GetPermissions());

        //    #endregion

        //    return permissionList;
        //}

        /// <summary>
        /// Get permission list about visit plugIn
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, bool>> GetVisitPermssions()
        {
            Dictionary<string, Dictionary<string, bool>> permissionList = new Dictionary<string, Dictionary<string, bool>>();

            #region Add permission to Dictionary
            // Add Visit Management page permission
            permissionList.Add(new VisitManagementPage().GetPageName(), new VisitManagementPage().GetPermissions());

            // Add Visit Report page permission
            permissionList.Add(new VisitReportPage().GetPageName(), new VisitReportPage().GetPermissions());

            // Add Visit Setting page permission
            permissionList.Add(new VisitSettingPage().GetPageName(), new VisitSettingPage().GetPermissions());

            #endregion

            return permissionList;
        }

        /// <summary>
        /// Get permission list about device message plugIn
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, bool>> GetDeviceMessagePermssions()
        {
            Dictionary<string, Dictionary<string, bool>> permissionList = new Dictionary<string, Dictionary<string, bool>>();

            #region Add permission to Dictionary
            // Add Device Message page permission
            permissionList.Add(new DeviceMessagePage().GetPageName(), new DeviceMessagePage().GetPermissions());

            #endregion

            return permissionList;
        }


        /// <summary>
        /// Get permission list about vehicle plugIn.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, bool>> GetVehiclePermssions()
        {
            Dictionary<string, Dictionary<string, bool>> permissionList = new Dictionary<string, Dictionary<string, bool>>();

            #region Add permission to Dictionary
            // Add Vehicle management page permission
            permissionList.Add(new VehicleManagementPage().GetPageName(), new VehicleManagementPage().GetPermissions());

            // Add Unit Vehicle allocation page permission
            permissionList.Add(new VehicleAllocationPage().GetPageName(), new VehicleAllocationPage().GetPermissions());

            #endregion

            return permissionList;
        }


        /// <summary>
        /// Get permission list about card issuing.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, bool>> GetCardIssuingPermssions()
        {
            Dictionary<string, Dictionary<string, bool>> permissionList = new Dictionary<string, Dictionary<string, bool>>();

            #region Add permission to Dictionary
            // Add Card Issuing page permission
            permissionList.Add(new CardIssuingPage().GetPageName(), new CardIssuingPage().GetPermissions());

            // Add Card Issuing Setting page permission
            permissionList.Add(new CardIssuingSettingPage().GetPageName(), new CardIssuingSettingPage().GetPermissions());

            // Add Card Issuing Layout page permission
            permissionList.Add(new CardIssuingLayoutPage().GetPageName(), new CardIssuingLayoutPage().GetPermissions());

            #endregion

            return permissionList;
        }

        /// <summary>
        /// Get permission list about event management.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, bool>> GetEventManagementPermissions()
        {
            Dictionary<string, Dictionary<string, bool>> permissionList = new Dictionary<string, Dictionary<string, bool>>();

            permissionList.Add(new EventManagementPage().GetPageName(), new EventManagementPage().GetPermissions());

            return permissionList;
        }

        /// <summary>
        /// Get permission list about book management.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, bool>> GetLibraryManagementPermissions()
        {
            Dictionary<string, Dictionary<string, bool>> permissionList = new Dictionary<string, Dictionary<string, bool>>();

            permissionList.Add(new BookPage().GetPageName(), new BookPage().GetPermissions());

            return permissionList;
        }

        /// <summary>
        /// Get permission list about army management plugIn
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, bool>> GetArmyPermissions()
        {
            Dictionary<string, Dictionary<string, bool>> permissionList = new Dictionary<string, Dictionary<string, bool>>();

            #region Add permission to Dictionary

            // Add army user management permission
            permissionList.Add(new UserArmyManagementPage().GetPageName(), new UserArmyManagementPage().GetPermissions());

            // Add army visit management permission
            permissionList.Add(new VisitArmyManagementPage().GetPageName(), new VisitArmyManagementPage().GetPermissions());

            #endregion

            return permissionList;
        }

    }

    public class Page
    {
        #region page list

        public const string DashBoard = "DashBoard";
        public const string Monitoring = "Monitoring";
        public const string DeviceMonitoring = "DeviceMonitoring";
        public const string VisitManagement = "Visit";
        public const string VisitReport = "VisitReport";
        public const string VisitSetting = "VisitSetting";
        public const string Department = "Department";
        public const string User = "User";

        public const string UnAssignUser = "UnAssignUser";

        public const string AccessSetting = "AccessSetting";
        public const string AccessGroup = "AccessGroup";
        public const string Timezone = "Timezone";
        public const string Holiday = "Holiday";
        public const string TimeAttendanceReport = "Attendance";
        public const string WorkingTime = "WorkingTime";
        public const string DeviceMessage = "DeviceMessage";
        public const string DeviceSetting = "DeviceSetting";
        public const string Building = "Building";
        public const string Report = "Report";
        public const string SystemLog = "SystemLog";
        public const string AccessibleDoor = "AccessibleDoor";
        public const string RegisteredUser = "RegisteredUser";
        public const string MealServiceTime = "MealServiceTime";

        public const string VehicleManagement = "Vehicle";
        public const string CardIssuing = "CardIssuing";
        public const string CardIssuingSetting = "CardIssuingSetting";
        public const string CardIssuingLayout = "CardIssuingLayout";
        public const string BookManagement = "Book";

        public const string VehicleAllocation = "VehicleAllocation";
        public const string VisitArmy = "VisitArmy";
        public const string UserArmy = "UserArmy";

        public const string Event = "Event";
        public const string EventManagement = "EventManagement";
        public const string LeaveRequest = "LeaveRequest";

        public const string Device = "Device";
        public const string UnassignDevice = "UnassignDevice";

        public const string BuildingMaster = "BuildingMaster";
        public const string UnassignBuildingMaster = "UnassignBuildingMaster";

        public const string DeviceReader = "DeviceReader";
        public const string AccessSchedule = "AccessSchedule";
        public const string Setting = "Setting";

        #endregion


        public string GetValue(string key)
        {
            //return GetType().GetField(key).GetDescription();
            return GetType().GetField(key).GetValue("").ToString();
        }
    }

    public class ActionName
    {
        #region action list

        public const string View = "View";
        public const string Add = "Add";
        public const string Edit = "Edit";
        public const string Delete = "Delete";

        public const string Export = "Export";

        public const string SendInstruction = "SendInstruction";
        public const string ViewHistory = "ViewHistory";
        public const string Reinstall = "Reinstall";
        public const string Reboot = "Reboot";
        public const string Copy = "Copy";

        public const string Approve = "Approve";
        public const string ReturnCard = "ReturnCard";

        public const string SetWorkingTime = "SetWorkingTime";

        public const string Run = "Run";

        public const string RegCivil = "RegCivil";
        public const string RegOther = "RegOther";

        public const string ViewAll = "ViewAll";

        public const string UpdateAttendanceSetting = "UpdateAttendanceSetting";
        public const string ViewLeaveManagement = "ViewLeaveManagement";
        public const string ManageOwnRecord = "ManageOwnRecord";

        #endregion
    }

    public class PagePermissionClass
    {
        public Dictionary<string, bool> GetPermissions()
        {
            var pageName = this.GetType().Name.Replace("Page", "");

            foreach (FieldInfo fi in typeof(Page).GetFields())
            {
                if (fi.Name == pageName)
                {
                    pageName = fi.GetValue((Object)fi.Name).ToString();
                    break;
                }
            }

            Dictionary<string, bool> permissions = new Dictionary<string, bool>();

            foreach (FieldInfo fi in GetType().GetFields())
            {
                permissions.Add(fi.Name + pageName, Convert.ToBoolean(fi.GetValue((object)fi.Name).ToString()));
            }

            return permissions;
        }

        public string GetPageName()
        {
            var pageName = this.GetType().Name.Replace("Page", "");

            return pageName;
        }

        public string GetPermission(string action)
        {
            var pageName = this.GetType().Name.Replace("Page", "");

            foreach (FieldInfo fi in GetType().GetFields())
            {
                if (fi.Name == action)
                    return fi.Name + new Page().GetValue(pageName);
            }

            return null;
        }
    }

    public class DashBoardPage : PagePermissionClass
    {
        public const bool View = true;
    }

    public class MonitoringPage : PagePermissionClass
    {
        public const bool View = true;
    }

    public class DeviceMonitoringPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool SendInstruction = false;
    }

    public class VisitManagementPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Add = true;
        public const bool Edit = true;
        public const bool Delete = false;
        public const bool Export = false;
        public const bool ViewHistory = true;
        public const bool Approve = true;
        public const bool ReturnCard = true;
    }

    public class VisitReportPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Export = false;
    }

    public class VisitSettingPage : PagePermissionClass
    {
        public const bool Edit = false;
    }

    public class DepartmentPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Add = false;
        public const bool Edit = false;
        public const bool Delete = false;
        public const bool ViewUser = false;
    }

    public class UserPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Add = false;
        public const bool Edit = false;
        public const bool Delete = false;
        public const bool Export = false;
        public const bool SetWorkingTime = false;
        public const bool Approve = false;
        public const bool UpdateUserSetting = false;
    }

    public class AccessSettingPage : PagePermissionClass
    {
        public const bool Edit = false;
    }

    public class AccessGroupPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Add = false;
        public const bool Edit = false;
        public const bool Delete = false;
    }

    public class TimezonePage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Add = false;
        public const bool Edit = false;
        public const bool Delete = false;
    }

    public class HolidayPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Add = false;
        public const bool Edit = false;
        public const bool Delete = false;
    }

    public class TimeAttendanceReportPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Edit = false;
        public const bool Export = false;
        public const bool ViewHistory = false;
        public const bool UpdateAttendanceSetting = false;
    }

    public class WorkingTimePage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Add = false;
        public const bool Edit = false;
        public const bool Delete = false;
    }

    public class DeviceSettingPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Edit = false;
        public const bool Copy = false;
        public const bool Reinstall = false;
        public const bool ViewHistory = false;
    }

    public class DeviceMessagePage : PagePermissionClass
    {
        public const bool Edit = false;
    }

    public class BuildingPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Add = false;
        public const bool Edit = false;
        public const bool Delete = false;

        public const bool ViewMaster = true;
        public const bool RegiMaster = false;
    }

    public class ReportPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool ViewAll = true;
        public const bool Export = false;
    }

    public class SystemLogPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Export = false;
    }

    public class AccessibleDoorPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Export = false;
    }

    public class RegisteredUserPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Export = false;
    }


    public class VehicleManagementPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Add = false;
        public const bool Edit = false;

        public const bool Delete = false;
    }

    public class VehicleAllocationPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Add = false;
        public const bool Edit = false;
        public const bool Delete = false;

        public const bool Run = false;
    }


    public class CardIssuingPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Add = false;
        public const bool Edit = false;
        public const bool Delete = false;
    }

    public class CardIssuingSettingPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Add = false;
        public const bool Edit = false;
        public const bool Delete = false;
    }

    public class CardIssuingLayoutPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Add = false;
        public const bool Edit = false;
        public const bool Delete = false;
    }

    public class EventManagementPage : PagePermissionClass
    {
        public const bool View = false;
        public const bool Add = false;
        public const bool Edit = false;
        public const bool Delete = false;
    }

    public class BookPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Add = false;
        public const bool Edit = false;
        public const bool Delete = false;
    }

    public class LeaveRequestPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Add = false;
        public const bool Edit = false;
        public const bool Delete = false;
        public const bool ViewLeaveManagement = false;
        public const bool ManageOwnRecord = true;
    }


    public class UserArmyManagementPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Add = false;
        public const bool Edit = false;
        public const bool Delete = false;
    }


    public class VisitArmyManagementPage : PagePermissionClass
    {
        public const bool RegCivil = true;
        public const bool RegOther = true;
    }

    public class DeviceReaderPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Add = false;
        public const bool Edit = false;
        public const bool Delete = false;
    }

    public class AccessSchedulePage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Add = false;
        public const bool Edit = false;
        public const bool Delete = false;
    }
    
    public class SettingPage : PagePermissionClass
    {
        public const bool View = true;
        public const bool Edit = true;
    }
}
