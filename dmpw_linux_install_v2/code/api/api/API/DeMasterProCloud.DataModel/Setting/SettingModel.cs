using System.Collections.Generic;
using Newtonsoft.Json;

namespace DeMasterProCloud.DataModel.Setting
{
    public class SettingEditModel
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public List<string> Value { get; set; }
    }
    public class SettingModel
    {
        public int Id { get; set; }
        [JsonIgnore]
        public string Key { get; set; }
        public List<string> Value { get; set; }
        [JsonIgnore]
        public string Category { get; set; }
    }

    public class SettingByCategoryModel
    {
        public string Category { get; set; }
        public List<FileSetting> Settings { get; set; }
    }

    public class FileSetting
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Key { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string[] Values { get; set; }
        public List<SettingOption> Options { get; set; }
    }

    public class SettingOption
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class LogoModel
    {
        public string Logo { get; set; }
    }

    public enum SettingType
    {
        Text,
        Boolean,
        MultipleSelection
    }

    public class AccountRabbitModel
    {
        public string UserName { get; set; }
        public string PassWord { get; set; }
    }
}
