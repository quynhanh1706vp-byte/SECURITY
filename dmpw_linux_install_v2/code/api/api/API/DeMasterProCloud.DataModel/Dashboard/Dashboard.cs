using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using DeMasterProCloud.Common.Resources;
using Newtonsoft.Json;
using System.Collections.Generic;
using DeMasterProCloud.Common.Infrastructure;
using DocumentFormat.OpenXml.Office2010.ExcelAc;

namespace DeMasterProCloud.DataModel.Dashboard
{
    public class AdminDashboardModel
    {
        
        public int TotalCompanies { get; set; }
        public int TotalOnlineDevices { get; set; }
        public int TotalOfflineDevices { get; set; }
        public int TotalDevices { get; set; }
        public int TotalAccounts { get; set; }
        public int TotalEventLogs { get; set; }
        public int TotalEventLogsToday { get; set; }
        public int TotalEventNormalAccess { get; set; }
        public int TotalEventAbNormalAccess { get; set; }
        public int TotalEventNormalAccessToday { get; set; }
        public int TotalEventAbNormalAccessToday { get; set; }
    }

    public class AccessDashboardModel
    {
        public int TotalUsers { get; set; }
        public int TotalUsersIn { get; set; }
        public int TotalUsersOut { get; set; }
        public int TotalUsersAccess { get; set; }
        public int TotalOnlineDevices { get; set; }
        public int TotalOfflineDevices { get; set; }
        public int TotalDevices { get; set; }
        public int TotalAccessEvents { get; set; }
        public int TotalAbnormalEvents { get; set; }
        public int TotalUnknownPerson { get; set; }
        public int TotalVisits { get; set; }
        public int TotalVisitsIn { get; set; }
        public int TotalVisitsOut { get; set; }

        public List<EventChartDataModel> EventChartData { get; set; }
        public List<EnumModel> DoorStatus { get; set; }
    }
    public class EventChartDataModel
    {
        public string DoorName { get; set; }
        public string BuildingName { get; set; }
        public int BuildingId { get; set; }
        public int DeviceId { get; set; }
        public List<int> InData { get; set; }
        public List<int> OutData { get; set; }
    }

    public class AttendanceTypeChart
    {
        public List<string> Labels { get; set; }
        public List<int> Data { get; set; }
    }


    public class AttendanceDashboardModel
    {

        public int TotalUserInOffice { get; set; }
        public int TotalLateUsers { get; set; }
        public int TotalAbsentUsers { get; set; }

        // public List<int> WorkingUsersChart { get; set; }
        public List<DataLineChart> WorkingUsersChart { get; set; }
        public AttendanceTypeChart AttendanceTypeChart { get; set; }
    }

    public class VisitDashboardModel
    {

        public int TotalVisitorAccess { get; set; }
        public int TotalAwaitingVisitors { get; set; }
        public int TotalRegisteredVisitor { get; set; }
        public int TotalVisits { get; set; }
        public int TotalVisitsIn { get; set; }
        public int TotalVisitsOut { get; set; }

        public List<EventChartDataModel> VisitChartData { get; set; }
    }

    public class CanteenDashboardModel
    {
        public int TotalUsers { get; set; }
        public int TotalUsersAccess { get; set; }
        public int TotalOnlineDevices { get; set; }
        public int TotalOfflineDevices { get; set; }
        public int TotalDevices { get; set; }
        public int TotalMealEvents { get; set; }

        public List<MealByCornerDataModel> MealByCornerChart { get; set; }
        public List<MealByServiceDataModel> MealByServiceChart { get; set; }
        public List<MealCountByType> MealCount { get; set; }
    }

    public class MealCountByType
    {
        public string MealType { get; set; }
        public int Count { get; set; }
    }

    public class MealByCornerDataModel
    {
        public string Corner { get; set; }
        public List<int> Data { get; set; }
        public List<string> Labels { get; set; }
    }
    public class MealByServiceDataModel
    {
        public string MealType { get; set; }
        public List<int> Data { get; set; }
        public List<string> Labels { get; set; }
    }

    public class DataLineChart
    {
        public string Label { get; set; }
        public List<int> Data { get; set; }
    }
}
