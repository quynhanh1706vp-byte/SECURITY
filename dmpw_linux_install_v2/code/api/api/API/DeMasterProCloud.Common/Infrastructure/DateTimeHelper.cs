using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using TimeZoneConverter;
using Microsoft.Extensions.Logging;

namespace DeMasterProCloud.Common.Infrastructure
{
    public static class DateTimeHelper
    {
        public static double TimeSpanToDouble(string timeSpan)
        {
            try
            {
                var hour = timeSpan.Split(':')[0];
                var minute = timeSpan.Split(':')[1];
                if (!string.IsNullOrEmpty(hour) && !string.IsNullOrEmpty(minute))
                {
                    if (double.Parse(hour) >= 24)
                    {
                        hour = Constants.Settings.MaxTimezoneTimeHour;
                    }
                    if (double.Parse(minute) >= 60)
                    {
                        minute = Constants.Settings.MaxTimezoneTimeMinute;
                    }
                    timeSpan = $"{hour}:{minute}";
                }
                return Convert.ToDouble(TimeSpan.Parse(timeSpan).TotalHours);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(DateTimeHelper));
                logger.LogError(ex, "Error in TimeSpanToDouble");
                return 0;
            }
        }

        public static string DoubleToTimeSpan(double dou)
        {
            try
            {
                if (dou >= 24)
                {
                    dou = Constants.Settings.MaxTimezoneTimeHourMinute;
                }
                return TimeSpan.FromHours(dou).ToString("h\\.mm");
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(DateTimeHelper));
                logger.LogError(ex, "Error in DoubleToTimeSpan");
                return string.Empty;
            }
        }

        /// <summary>
        /// Method checks if passed string is datetime
        /// </summary>
        /// <param name="dateTime">string text for checking</param>
        /// <returns>bool - if text is datetime return true, else return false</returns>
        public static bool IsDateTime(string dateTime)
        {
            try
            {
                // Allow DateTime string is null
                if (string.IsNullOrEmpty(dateTime))
                {
                    return true;
                }

                return dateTime.ConvertDefaultStringToDateTime() != null || DateTime.TryParse(dateTime, out _);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(DateTimeHelper));
                logger.LogError(ex, "Error in IsDateTime");
                return false;
            }
        }

        /// <summary>
        /// Method checks if passed string is datetime
        /// </summary>
        /// <param name="dateTime">string text for checking</param>
        /// <param name="format"></param>
        /// <returns>bool - if text is datetime return true, else return false</returns>
        public static bool IsDateTime(string dateTime, string format)
        {
            try
            {
                // Allow DateTime string is null
                if (string.IsNullOrEmpty(dateTime))
                {
                    return true;
                }

                return DateTime.TryParseExact(dateTime, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(DateTimeHelper));
                logger.LogError(ex, "Error in IsDateTime(format)");
                return false;
            }
        }

        /// <summary>
        /// Check expireDate and effective date
        /// </summary>
        /// <param name="expireDate"></param>
        /// <param name="effectiveDate"></param>
        /// <returns></returns>
        public static bool CheckDateTime(string expireDate, string effectiveDate)
        {
            try
            {
                var expire = expireDate.ConvertDefaultStringToDateTime();
                var effective = effectiveDate.ConvertDefaultStringToDateTime();
                return expire >= effective;
            }
            catch
            {
                var expire = Convert.ToDateTime(expireDate);
                var effective = Convert.ToDateTime(effectiveDate);
                return expire >= effective;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        public static DateTime ConvertAccessTime(string date)
        {
            try
            {
                if (string.IsNullOrEmpty(date))
                {
                    return new DateTime();
                }
                return DateTime.ParseExact(date, Constants.DateTimeFormat.DdMMyyyyHHmmss, null);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(DateTimeHelper));
                logger.LogError(ex, "Error in ConvertAccessTime");
                return DateTime.MinValue;
            }
        }
        
        // Convert Date Time to universal time and convert by timezone
        public static DateTime ConvertDateTimeByTimeZoneToSystemTimeZone(string date, string timeZone)
        {
            try
            {
                if (string.IsNullOrEmpty(date))
                {
                    return new DateTime();
                }

                TimeZoneInfo cstZone = timeZone.ToTimeZoneInfo();

                var utc = cstZone.BaseUtcOffset.ToString().Split(":");
                var newUtc = "";
                if (utc[0].StartsWith("-"))
                {
                    newUtc = utc[0] + utc[1];
                }
                else
                {
                    newUtc = "+" + utc[0] + utc[1];
                }

                var newDate = date + newUtc;
                var convertDateTime = DateTime.ParseExact(newDate, Constants.DateTimeFormat.ddMMyyyyHHmmsszzz, null).ToUniversalTime();
                return convertDateTime;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(DateTimeHelper));
                logger.LogError(ex, "Error in ConvertDateTimeByTimeZoneToSystemTimeZone");
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Get UTC time
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string GetUTCtime(string timeZone)
        {
            try
            {
                TimeZoneInfo cstZone = timeZone.ToTimeZoneInfo();

                var utc = cstZone.BaseUtcOffset.ToString().Split(":");

                if (utc[0].StartsWith("-"))
                {
                    return (Int32.Parse(utc[0]) * 60 + Int32.Parse(utc[1])).ToString();
                }
                else
                {
                    return "+" + (Int32.Parse(utc[0]) * 60 + Int32.Parse(utc[1])).ToString();
                }
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(DateTimeHelper));
                logger.LogError(ex, "Error in GetUTCtime");
                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        public static DateTime ConvertRecoveryDateTime(string date)
        {
            try
            {
                if (string.IsNullOrEmpty(date))
                {
                    return new DateTime();
                }
                return DateTime.ParseExact(date, Constants.DateTimeFormat.MMddyyyyHHmmss, null);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(DateTimeHelper));
                logger.LogError(ex, "Error in ConvertRecoveryDateTime");
                return DateTime.MinValue;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        public static DateTime ConvertToDateTime(string date)
        {
            try
            {
                if (string.IsNullOrEmpty(date))
                {
                    return new DateTime();
                }
                return DateTime.ParseExact(date, Constants.DateTimeFormat.DdMMyyyy, null);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(DateTimeHelper));
                logger.LogError(ex, "Error in ConvertToDateTime");
                return DateTime.MinValue;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static string ChangeFormatDatetime(string datetime)
        {
            try
            {
                if (string.IsNullOrEmpty(datetime))
                {
                    return string.Empty;
                }

                var date = datetime.Length == 12
                    ? DateTime.ParseExact(datetime, Constants.DateTimeFormat.DdMMyyyyHHmm, null)
                    : DateTime.ParseExact(datetime, Constants.DateTimeFormat.DdMMyyyy, null);
                return date.ToString(Constants.DateTimeFormat.DdMdYyyyFormat);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(DateTimeHelper));
                logger.LogError(ex, "Error in ChangeFormatDatetime");
                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        public static DateTime ConvertUpdateTime(string date)
        {
            try
            {
                if (string.IsNullOrEmpty(date))
                {
                    return new DateTime();
                }
                return DateTime.ParseExact(date, Constants.DateTimeFormat.DdMMyyyyHHmmssFfffff, null);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(DateTimeHelper));
                logger.LogError(ex, "Error in ConvertUpdateTime");
                return DateTime.MinValue;
            }
        }


        public static DateTime ConverStringToDateTime(string date)
        {
            try
            {
                if (string.IsNullOrEmpty(date))
                {
                    return new DateTime();
                }
                return DateTime.ParseExact(date, Constants.DateTimeFormat.YyyyyMdDdFormat, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(DateTimeHelper));
                logger.LogError(ex, "Error in ConverStringToDateTime");
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Get list date from range date
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static List<DateTime> GetListRangeDate(DateTime startDate, DateTime endDate)
        {
            try
            {
                return Enumerable.Range(0, 1 + endDate.Subtract(startDate).Days)
                    .Select(offset => startDate.AddDays(offset))
                    .ToList();
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(DateTimeHelper));
                logger.LogError(ex, "Error in GetListRangeDate");
                return new List<DateTime>();
            }
        }

        public static DateTime ConvertIsoToDateTime(string startDate)
        {
            try
            {
                CultureInfo culture = CultureInfo.InvariantCulture;
                DateTime time;
                DateTime.TryParse(startDate, culture, DateTimeStyles.None, out time);
                return time;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(DateTimeHelper));
                logger.LogError(ex, "Error in ConvertIsoToDateTime");
                return DateTime.MinValue;
            }
        }
        
        public static string ConvertDateTimeToIso(DateTime dateTime)
        {
            try
            {
                CultureInfo culture = CultureInfo.InvariantCulture;
                string s2 = dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", culture);
                return s2;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(DateTimeHelper));
                logger.LogError(ex, "Error in ConvertDateTimeToIso");
                return string.Empty;
            }
        }

        public static long ConvertToTimeSpanUnix(this DateTime dateTime)
        {
            try
            {
                DateTimeOffset dateTimeOffset = dateTime;
                return dateTimeOffset.ToUnixTimeMilliseconds();
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(DateTimeHelper));
                logger.LogError(ex, "Error in ConvertToTimeSpanUnix");
                return 0;
            }
        }
        
        public static string ConvertDefaultDateTimeToString(this DateTime date, string format = Constants.Settings.DateTimeFormatDefault)
        {
            try
            {
                return date.ToString(format);
            }
            catch
            {
                return null;
            }
        }
        
        public static DateTime? ConvertDefaultStringToDateTime(this string date, string format = Constants.Settings.DateTimeFormatDefault)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(date)) return null;

                if (date.Length <= "dd.MM.yyyy".Length) date = date.Trim() + " 00:00:00";
            
                return DateTime.SpecifyKind(DateTime.ParseExact(date, format, null), DateTimeKind.Utc);
            }
            catch
            {
                return null;
            }
        }
        public static DateTime? ConvertDefaultStringToDateTimeWithFormat(this string date, string format)
        {
            try
            {
                return DateTime.SpecifyKind(DateTime.ParseExact(date, format, null), DateTimeKind.Utc);
            }
            catch
            {
                return null;
            }
        }
    }
}