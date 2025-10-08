using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace DeMasterProCloud.Common.Infrastructure
{
    public static class EnumHelper
    {
        public static SelectList ToSelectList<T>(short? selected) where T : struct
        {
            try
            {
                return selected == null ? ToSelectList<T>() : ToSelectList<T>(selected.Value);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(EnumHelper));
                logger.LogError(ex, "Error in ToSelectList");
                return null;
            }
        }
        public static SelectList ToSelectList<T>() where T : struct
        {
            try
            {
                Type t = typeof(T);
                if (t.IsEnum)
                {
                    var values = Enum.GetValues(typeof(T)).Cast<Enum>().Select(e => new
                    {
                        Id = Convert.ToInt32(e),
                        Name = e.GetDescription()
                    });
                    return new SelectList(values, nameof(EnumModel.Id), nameof(EnumModel.Name));
                }
                return null;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(EnumHelper));
                logger.LogError(ex, "Error in ToSelectList");
                return null;
            }
        }

        public static IEnumerable<SelectListItem> ToStringSelectList<T>() where T : struct
        {
            try
            {
                Type t = typeof(T);
                if (t.IsEnum)
                {
                    var values = Enum.GetValues(typeof(T)).Cast<Enum>().Select(e => new
                    {
                        Id = e.GetName(),
                        Name = e.GetDescription()
                    });
                    return new SelectList(values, nameof(EnumModel.Id), nameof(EnumModel.Name));
                }
                return null;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(EnumHelper));
                logger.LogError(ex, "Error in ToStringSelectList");
                return null;
            }
        }

        public static SelectList ToSelectList<T>(short selected) where T : struct
        {
            try
            {
                Type t = typeof(T);
                if (t.IsEnum)
                {
                    var values = Enum.GetValues(t).Cast<Enum>().Select(e => new
                    {
                        Id = Convert.ToInt32(e),
                        Name = e.GetDescription()
                    });
                    return new SelectList(values, nameof(EnumModel.Id), nameof(EnumModel.Name),selected);
                }
                return null;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(EnumHelper));
                logger.LogError(ex, "Error in ToSelectList(selected)");
                return null;
            }
        }

        public static List<EnumModel> ToEnumList<T>()
        {
            try
            {
                List<EnumModel> enumModels = new List<EnumModel>();
                Type t = typeof(T);
                if (t.IsEnum)
                {
                    var values = Enum.GetValues(t).Cast<Enum>().Select(e => new
                    {
                        Id = Convert.ToInt32(e),
                        Name = e.GetDescription()
                    });

                    foreach (var item in values)
                    {
                        enumModels.Add(new EnumModel { Id = item.Id, Name = item.Name });
                    }
                }
                return enumModels;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(EnumHelper));
                logger.LogError(ex, "Error in ToEnumList");
                return new List<EnumModel>();
            }
        }

        public static List<EnumModelWithValue> ToEnumListWithValue<T>()
        {
            try
            {
                List<EnumModelWithValue> enumModels = new List<EnumModelWithValue>();
                Type t = typeof(T);
                if (t.IsEnum)
                {
                    var values = Enum.GetValues(t).Cast<Enum>().Select(e => new
                    {
                        Id = Convert.ToInt32(e),
                        Name = e.GetDescription(),
                        Value = e.GetName()
                    });

                    foreach (var item in values)
                    {
                        enumModels.Add(new EnumModelWithValue { Id = item.Id, Name = item.Name, Value = item.Value });
                    }
                }
                return enumModels;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(EnumHelper));
                logger.LogError(ex, "Error in ToEnumListWithValue");
                return new List<EnumModelWithValue>();
            }
        }

        public static List<dynamic> ToEnumListText<T>()
        {
            try
            {
                List<dynamic> enumModels = new List<dynamic>();
                Type t = typeof(T);
                if (t.IsEnum)
                {
                    var values = Enum.GetValues(t).Cast<Enum>().Select(e => new
                    {
                        Id = e.GetDescription(),
                        Name = e.GetDescription()
                    });

                    foreach (var item in values)
                    {
                        enumModels.Add(new {item.Id, item.Name });
                    }
                }
                return enumModels;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(EnumHelper));
                logger.LogError(ex, "Error in ToEnumListText");
                return new List<dynamic>();
            }
        }

        public static string GetDescription<TEnum>(this TEnum value)
        {
            try
            {
                FieldInfo fi = value.GetType().GetField(value.ToString());
                if (fi != null)
                {
                    DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    if (attributes.Length > 0)
                    {
                        return attributes[0].Description;
                    }
                }
                return value.ToString();
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(EnumHelper));
                logger.LogError(ex, "Error in GetDescription");
                return string.Empty;
            }
        }

        public static List<string> GetAllDescriptions<TEnum>(int value, ResourceManager resourceManager)
        {
            try
            {
                var enumName = Enum.GetName(typeof(TEnum), value);
                if (enumName == null)
                {
                    return new List<string>(value);
                }
                else if(resourceManager.GetString($"lbl{enumName}") != null)
                {
                    List<string> values = new List<string>();
                    List<string> languages = new List<string>() { "en-US", "ja-JP", "ko-KR", "vi-VN", "af-NA" };
                    foreach (var language in languages)
                    {
                        values.Add(resourceManager.GetString($"lbl{enumName}", new CultureInfo(language)));
                    }

                    return values;
                }
                else
                {
                    return new List<string>();
                }
            }
            catch (Exception e)
            {
                return new List<string>();
            }
        }

        public static string GetName<TEnum>(this TEnum value)
        {
            try
            {
                return value.GetType().GetMember(value.ToString()).First().Name;
            }
            catch (Exception e)
            {
                return "";
            }
        }
        //public static string GetDisplayName<TEnum>(this TEnum value)
        //{
        //    FieldInfo fi = value.GetType().GetField(value.ToString());
        //    if (fi != null)
        //    {
        //        DisplayAttribute attribute = (DisplayAttribute)fi.GetCustomAttribute(typeof(DisplayAttribute), false);
        //        return attribute.Name ?? string.Empty;
        //    }
        //    return value.ToString();
        //}
        public static bool IsEnum<T>(int selected) where T : struct
        {
            try
            {
                SelectList list = ToSelectList<T>();
                foreach (var item in list)
                {
                    if (Convert.ToInt32(item.Value) == selected) { return true; }
                }
                return false;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(EnumHelper));
                logger.LogError(ex, "Error in IsEnum");
                return false;
            }
        }
        //public static bool IsEnum<T>(string multiSelected) where T : struct
        //{
        //    if (string.IsNullOrWhiteSpace(multiSelected)) { return false; }
        //    if (multiSelected.IndexOf(" ", StringComparison.Ordinal) > -1) { return false; }

        //    char[] delimiterChars = { ',' };
        //    int[] selecteds = multiSelected.Split(delimiterChars).Select(x => Convert.ToInt32(x)).ToArray();
        //    int nSelecteds = selecteds.Distinct().Count();
        //    if (nSelecteds == selecteds.Length) { return false; }

        //    foreach (var item in selecteds)
        //    {
        //        if (!IsEnum<T>(item)) { return false; }
        //    }
        //    return true;
        //}

        public static int GetValueByName(Type type, string name)
        {
            try
            {
                int value = (int)Enum.Parse(type, name);
                return value;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        
        public static Dictionary<string, List<string>> GetAllDescriptionWithEnum<T>(ResourceManager resourceManager)
        {
            try
            {
                Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();

                Type t = typeof(T);
                var names = Enum.GetNames(t);

                foreach (string name in names)
                {
                    List<string> values = GetAllDescriptions<T>((int)Enum.Parse(t, name), resourceManager);
                    result.Add(name, values);
                }

                return result;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(EnumHelper));
                logger.LogError(ex, "Error in GetAllDescriptionWithEnum");
                return new Dictionary<string, List<string>>();
            }
        }

        public static Dictionary<short, List<string>> GetAllDescriptionWithId<T>(ResourceManager resourceManager)
        {
            try
            {
                Dictionary<short, List<string>> result = new Dictionary<short, List<string>>();

                Type t = typeof(T);
                var names = Enum.GetNames(t);
                var items = Enum.GetValues(t).Cast<Enum>().Select(e => new
                {
                    Id = Convert.ToInt32(e),
                    Name = e.GetName()
                });

                foreach (string name in names)
                {
                    List<string> values = GetAllDescriptions<T>((int)Enum.Parse(t, name), resourceManager);
                    var item = items.FirstOrDefault(e => e.Name == name);
                    if(item != null)
                        result.Add((short) item.Id, values);
                }

                return result;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(EnumHelper));
                logger.LogError(ex, "Error in GetAllDescriptionWithId");
                return new Dictionary<short, List<string>>();
            }
        }
    }
    public class EnumModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class EnumModelWithValue
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public class EnumModelDeviceType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<EnumModel> VerifyModeList { get; set; }
    }

    public class AutoComplete
    {
        public int Value { get; set; }
        public string Label { get; set; }
    }
}
