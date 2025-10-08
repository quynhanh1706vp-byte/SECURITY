using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DeMasterProCloud.DataModel.Role
{
    public class RoleModel
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
        public bool IsDefault { get; set; }
        public bool EnableDepartmentLevel { get; set; }
        public bool RoleSettingDefault { get; set; }
        public string Description { get; set; }
        public int UserCount { get; set; }
        public List<PermissionGroupModel> PermissionGroups { get; set; }
    }

    public class PermissionGroupModel
    {
        public string Title { get; set; }
        public string GroupName { get; set; }
        public List<PermissionModel> Permissions { get; set; }
    }

    public class PermissionModel
    {
        public string Title { get; set; }
        public string PermissionName { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
    }


    public class RoleDataModel
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
        public List<PermissionGroupDataModel> PermissionGroups { get; set; }
    }

    public class PermissionGroupDataModel
    {
        public string Title { get; set; }
        public List<PermissionDataModel> Permissions { get; set; }
    }

    public class PermissionDataModel
    {
        public string Title { get; set; }
        public bool IsEnabled { get; set; }
    }

}