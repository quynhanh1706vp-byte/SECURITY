using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using TimeZoneConverter;
using ImageMagick;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using DeMasterProCloud.Common.Resources;

namespace DeMasterProCloud.Common.Infrastructure
{
    public static class Helpers
    {
        private static readonly Random Random = new Random();

        /// <summary>
        /// Check a file extension is in valid extensions or not.
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="validExtensions"></param>
        /// <returns></returns>
        public static bool IsValidImage(string extension, params string[] validExtensions)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }
            return validExtensions.Contains(extension);
        }

        /// <summary>
        /// Convert date  to specify setting date string
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ToSettingDateString(this DateTime date)
        {
            var culture = Thread.CurrentThread.CurrentCulture.Name;
            string dateFormat = ApplicationVariables.Configuration[Constants.DateServerFormat + ":" + culture];
            return date.ToString(dateFormat);
        }
        /// <summary>
        /// This function is used to apply timezone to DateTime form data.
        /// EXAMPLE) 00:34:17 ( UTC 0 ) -> 09:34:17 ( UTC 9 )
        /// This function is made for speed performance.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="userTimeZone"></param>
        /// <returns></returns>
        public static DateTime ConvertToUserTime(this DateTime date, TimeSpan offSet)
        {
            //if (date == DateTime.MinValue || DateTime.MaxValue - date <= new TimeSpan(0, 0, 1)) return date;

            // var culture = Thread.CurrentThread.CurrentCulture.Name;
            // string dateFormat = ApplicationVariables.Configuration[Constants.DateServerFormat + ":" + culture];

            try
            {
                if (offSet.TotalMilliseconds > 0)
                {
                    if (DateTime.MaxValue - date <= new TimeSpan(0, 0, 1)) return date;
                }
                else if (offSet.TotalMilliseconds < 0)
                {
                    if (date == DateTime.MinValue) return date;
                }

                DateTime cstTime = date.AddSeconds(offSet.TotalSeconds);

                return cstTime;
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (ArgumentOutOfRangeException)
            {
                var maxDiff = DateTime.MaxValue - date;
                var minDiff = date - DateTime.MinValue;

                if (maxDiff <= offSet)
                {
                    date = DateTime.MaxValue;
                }
                else if (minDiff <= offSet)
                {
                    date = DateTime.MinValue;
                }
            }

            return date;
        }

        /// <summary>
        /// This function is used to apply timezone to DateTime form data.
        /// EXAMPLE) 00:34:17 ( UTC 0 ) -> 09:34:17 ( UTC 9 )
        /// </summary>
        /// <param name="date"></param>
        /// <param name="userTimeZone"></param>
        /// <returns></returns>
        public static DateTime ConvertToUserTime(this DateTime date, string userTimeZone = "")
        {
            //if (date == DateTime.MinValue || DateTime.MaxValue - date <= new TimeSpan(0, 0, 1)) return date;

            // var culture = Thread.CurrentThread.CurrentCulture.Name;
            // string dateFormat = ApplicationVariables.Configuration[Constants.DateServerFormat + ":" + culture];

            if (!String.IsNullOrEmpty(userTimeZone))
            {
                TimeZoneInfo cstZone;

                try
                {
                    cstZone = userTimeZone.ToTimeZoneInfo();

                    var offSet = cstZone.BaseUtcOffset;
                    if (offSet.TotalMilliseconds > 0)
                    {
                        if (DateTime.MaxValue - date <= new TimeSpan(0, 0, 1)) return date;
                    }
                    else if (offSet.TotalMilliseconds < 0)
                    {
                        if (date == DateTime.MinValue) return date;
                    }

                    var dt = DateTime.UtcNow;
                    var utcOffset = new DateTimeOffset(dt, TimeSpan.Zero);

                    var dateTimeOffset = utcOffset.ToOffset(cstZone.GetUtcOffset(utcOffset));

                    //DateTime cstTime = date.AddSeconds(cstZone.BaseUtcOffset.TotalSeconds);
                    DateTime cstTime = date.AddSeconds(dateTimeOffset.Offset.TotalSeconds);

                    return cstTime;
                }
                catch (TimeZoneNotFoundException)
                {
                }
                catch (ArgumentOutOfRangeException)
                {
                    cstZone = userTimeZone.ToTimeZoneInfo();

                    var maxDiff = DateTime.MaxValue - date;
                    var minDiff = date - DateTime.MinValue;

                    if (maxDiff <= cstZone.BaseUtcOffset)
                    {
                        date = DateTime.MaxValue;
                    }
                    else if (minDiff <= cstZone.BaseUtcOffset)
                    {
                        date = DateTime.MinValue;
                    }
                }
            }
            return date;
        }

        /// <summary>
        /// This function is used to apply timezone to DateTime form data.
        /// User timezone -> System timezone
        /// EXAMPLE) 09:34:17 ( UTC 9 ) -> 00:34:17 ( UTC 0 )
        /// This function is made for speed performance.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="userTimeZone"></param>
        /// <returns></returns>
        public static DateTime ConvertToSystemTime(this DateTime date, TimeSpan offSet)
        {
            //if (date == DateTime.MinValue || DateTime.MaxValue - date <= new TimeSpan(0, 0, 1)) return date;

            // var culture = Thread.CurrentThread.CurrentCulture.Name;
            // string dateFormat = ApplicationVariables.Configuration[Constants.DateServerFormat + ":" + culture];

            try
            {
                if (offSet.TotalMilliseconds > 0)
                {
                    if (date == DateTime.MinValue) return date;
                }
                else if (offSet.TotalMilliseconds < 0)
                {
                    if (DateTime.MaxValue - date <= new TimeSpan(0, 0, 1)) return date;
                }

                DateTime cstTime = date.Subtract(offSet);

                return cstTime;
            }
            catch (TimeZoneNotFoundException)
            {

            }
            catch (ArgumentOutOfRangeException)
            {
                var maxDiff = DateTime.MaxValue - date;
                var minDiff = date - DateTime.MinValue;

                if (maxDiff <= offSet)
                {
                    date = DateTime.MaxValue;
                }
                else if (minDiff <= offSet)
                {
                    date = DateTime.MinValue;
                }
            }

            return date;
        }

        /// <summary>
        /// This function is used to apply timezone to DateTime form data.
        /// User timezone -> System timezone
        /// EXAMPLE) 09:34:17 ( UTC 9 ) -> 00:34:17 ( UTC 0 )
        /// </summary>
        /// <param name="date"></param>
        /// <param name="userTimeZone"></param>
        /// <returns></returns>
        public static DateTime ConvertToSystemTime(this DateTime date, string userTimeZone = "")
        {
            //if (date == DateTime.MinValue || DateTime.MaxValue - date <= new TimeSpan(0, 0, 1)) return date;

            // var culture = Thread.CurrentThread.CurrentCulture.Name;
            // string dateFormat = ApplicationVariables.Configuration[Constants.DateServerFormat + ":" + culture];

            if (!String.IsNullOrEmpty(userTimeZone))
            {
                TimeZoneInfo cstZone;
                DateTimeOffset dateTimeOffset;

                try
                {
                    cstZone = userTimeZone.ToTimeZoneInfo();

                    var offSet = cstZone.BaseUtcOffset;
                    if (offSet.TotalMilliseconds > 0)
                    {
                        if (date == DateTime.MinValue) return date;
                    }
                    else if (offSet.TotalMilliseconds < 0)
                    {
                        if (DateTime.MaxValue - date <= new TimeSpan(0, 0, 1)) return date;
                    }

                    var result = TimeZoneInfo.ConvertTimeToUtc(date, cstZone);

                    return result;

                    ////var dt = DateTime.UtcNow;
                    ////var utcOffset = new DateTimeOffset(dt, TimeSpan.Zero);
                    //var utcOffset = new DateTimeOffset(date, TimeSpan.Zero);

                    ////dateTimeOffset = utcOffset.ToOffset(cstZone.GetUtcOffset(utcOffset));
                    //dateTimeOffset = utcOffset.ToOffset(cstZone.GetUtcOffset(utcOffset));

                    ////DateTime cstTime = date.Subtract(cstZone.BaseUtcOffset);
                    //DateTime cstTime = date.Subtract(dateTimeOffset.Offset);

                    //return cstTime;
                }
                catch (TimeZoneNotFoundException)
                {

                }
                catch (ArgumentOutOfRangeException)
                {
                    cstZone = userTimeZone.ToTimeZoneInfo();

                    var dt = DateTime.UtcNow;
                    var utcOffset = new DateTimeOffset(dt, TimeSpan.Zero);

                    dateTimeOffset = utcOffset.ToOffset(cstZone.GetUtcOffset(utcOffset));

                    var maxDiff = DateTime.MaxValue - date;
                    var minDiff = date - DateTime.MinValue;

                    //if (maxDiff <= cstZone.BaseUtcOffset)
                    if (maxDiff <= dateTimeOffset.Offset)
                    {
                        date = DateTime.MaxValue;
                    }
                    //else if (minDiff <= cstZone.BaseUtcOffset)
                    else if (minDiff <= dateTimeOffset.Offset)
                    {
                        date = DateTime.MinValue;
                    }
                }
                catch (Exception ex)
                {
                    if (!string.IsNullOrEmpty(userTimeZone))
                    {
                        int hours = GetUtcOffSetHour(userTimeZone);
                        int minutes = GetUtcOffSetMinute(userTimeZone);
                        return date.AddHours(-hours).AddMinutes(-minutes);
                    }
                }
            }
            return date;
        }

        public static string ConvertToUserTimeZone(this DateTime date, string userTimeZone = "")
        {
            var culture = Thread.CurrentThread.CurrentCulture.Name;
            string dateFormat = ApplicationVariables.Configuration[Constants.DateServerFormat + ":" + culture];

            if (!String.IsNullOrEmpty(userTimeZone))
            {
                try
                {
                    TimeZoneInfo cstZone = userTimeZone.ToTimeZoneInfo();

                    DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(date.ToUniversalTime(), cstZone);
                    return cstTime.ToString(dateFormat);
                }
                catch (TimeZoneNotFoundException)
                {
                }
            }
            return date.ToString(dateFormat);
        }

        /// <summary>
        /// Convert date  to specify setting date string
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ToSettingDateTimeString(this DateTime date)
        {
            var culture = Thread.CurrentThread.CurrentCulture.Name;
            string dateFormat;

            try
            {
                if (ApplicationVariables.Configuration != null)
                {
                    dateFormat = ApplicationVariables.Configuration[Constants.DateTimeServerFormat + ":" + culture];
                }
                else
                {
                    // If ApplicationVariables.Configuration is null, API set the date format as MM.dd.yyyy HH:mm:ss (en-US)
                    // Sometimes ApplicationVariables.Configuration is null when the server is started.
                    dateFormat = "MM.dd.yyyy HH:mm:ss";
                }
            }
            catch (Exception)
            {
                dateFormat = "MM.dd.yyyy HH:mm:ss";
            }

            return date.ToString(dateFormat);
        }
        
        public static string ToSettingDateTimeString(this DateTime date, string language = null)
        {
            var culture = string.IsNullOrEmpty(language) ? Thread.CurrentThread.CurrentCulture.Name : language;
            string dateFormat;

            try
            {
                if (ApplicationVariables.Configuration != null)
                {
                    dateFormat = ApplicationVariables.Configuration[Constants.DateTimeServerFormat + ":" + culture];
                }
                else
                {
                    // If ApplicationVariables.Configuration is null, API set the date format as MM.dd.yyyy HH:mm:ss (en-US)
                    // Sometimes ApplicationVariables.Configuration is null when the server is started.
                    dateFormat = "MM.dd.yyyy HH:mm:ss";
                }
            }
            catch (Exception)
            {
                dateFormat = "MM.dd.yyyy HH:mm:ss";
            }

            return date.ToString(dateFormat);
        }

        public static double ToSettingDateTimeUnique(this DateTime date)
        {
            var epoch = (date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            return epoch;
        }


        /// <summary>
        /// Convert date  to specify setting date string
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ToSettingDateTimeString(this DateTime? date)
        {
            var culture = Thread.CurrentThread.CurrentCulture.Name;
            string dateFormat = ApplicationVariables.Configuration[Constants.DateTimeServerFormat + ":" + culture];
            return date?.ToString(dateFormat);
        }

        /// <summary>
        /// Convert date to specify setting date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ToSettingDateString(this DateTime? date)
        {
            var culture = Thread.CurrentThread.CurrentCulture.Name;
            string dateFormat = ApplicationVariables.Configuration[Constants.DateServerFormat + ":" + culture];
            return date?.ToString(dateFormat);
        }

        /// <summary>
        /// Get date server format by current culture
        /// </summary>
        /// <returns></returns>
        public static string GetDateServerFormat()
        {
            var culture = Thread.CurrentThread.CurrentCulture.Name;
            return ApplicationVariables.Configuration[Constants.DateServerFormat + ":" + culture];
        }

        /// <summary>
        /// Get company id
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static int GetCompanyId(this ClaimsPrincipal user)
        {
            int.TryParse(user.Claims.FirstOrDefault(m => m.Type == Constants.ClaimName.CompanyId)?.Value, out var companyId);
            return companyId;
        }

        /// <summary>
        /// Get company code
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetCompanyCode(this ClaimsPrincipal user)
        {
            return user.Claims.FirstOrDefault(m => m.Type == Constants.ClaimName.CompanyCode)?.Value;
        }

        /// <summary>
        /// Get User Name
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetUsername(this ClaimsPrincipal user)
        {
            return user.Claims.FirstOrDefault(m => m.Type == Constants.ClaimName.Username)?.Value;
        }

        /// <summary>
        /// Get account type
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static short GetAccountType(this ClaimsPrincipal user)
        {
            short.TryParse(
                user.Claims.FirstOrDefault(m => m.Type == Constants.ClaimName.AccountType)?.Value, out var value);
            return value;
        }

        /// <summary>
        /// Get account id
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static int GetAccountId(this ClaimsPrincipal user)
        {
            int.TryParse(
                user.Claims.FirstOrDefault(m => m.Type == Constants.ClaimName.AccountId)?.Value, out var value);
            return value;
        }

        public static string GetTimezone(this ClaimsPrincipal user)
        {
            try
            {
                return user.Claims.FirstOrDefault(m => m.Type == Constants.ClaimName.Timezone)?.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public static string GetTokenBearer(this HttpContext context)
        {
            if (context != null && context.Request.Headers.TryGetValue("Authorization", out var authorization))
            {
                var token = authorization.ToString();
                if (token.Length > 10 && token.Substring(0, 6).ToLower() == "bearer")
                {
                    return token.Substring(6).Trim();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Generate company code
        /// </summary>
        /// <returns></returns>
        public static string GenerateCompanyCode()
        {
            var index = Random.Next(Constants.Alphabet.Length);
            var firstChar = Constants.Alphabet[index];

            var randomNumber = Enumerable.Range(0, 9)
                .OrderBy(x => Random.Next())
                .Take(6);

            return $"{firstChar}{string.Join(string.Empty, randomNumber)}";
        }

        /// <summary>
        /// Hex string to byte array
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] HexStringToByteArray(string hex)
        {
            try
            {
                var numberChars = hex.Length;
                var bytes = new byte[numberChars / 2];
                for (var i = 0; i < numberChars; i += 2)
                {
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                }
                return bytes;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(Helpers));
                logger.LogError(ex, "Error in HexStringToByteArray");
                return null;
            }
        }

        /// <summary>
        /// Generates a Random Password
        /// respecting the given strength requirements.
        /// </summary>
        /// <param name="opts">A valid PasswordOptions object
        /// containing the password strength requirements.</param>
        /// <returns>A random password</returns>
        public static string GenerateRandomPassword(PasswordOptions opts = null)
        {
            if (opts == null) opts = new PasswordOptions()
            {
                RequiredLength = 8,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = false,
                RequireUppercase = true
            };

            var randomChars = Constants.RandomChars;
            var rand = new Random(Environment.TickCount);
            var chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (var i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                var rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

        public static string GeneratePasswordDefaultWithCompany(string companyName)
        {
            var names = companyName.ReplaceSpacesString().Split(" ");
            string password = "";
            foreach (var item in names)
            {
                if (KoreanHelpers.IsKorean(item[0]))
                {
                    string[] elementArray = KoreanHelpers.DivideKorean(item[0]);
                    string[] elementArrayEng = KoreanHelpers.ConvertKeyboardEng(elementArray);

                    if (elementArrayEng == null)
                        elementArrayEng = elementArray;

                    password += string.Join("", elementArrayEng);
                }
                else
                {
                    password += char.ToLower(item[0]);
                }
            }

            password += DateTime.Now.Year;
            return password;
        }

        /// <summary>
        /// Comparre date
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static bool CompareDate(string startDate, string endDate)
        {
            try
            {
                if (DateTimeHelper.IsDateTime(startDate) && DateTimeHelper.IsDateTime(endDate))
                {
                    return Convert.ToDateTime(endDate).Date.Subtract(Convert.ToDateTime(startDate).Date).Days >= 0;
                }
                return true;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(Helpers));
                logger.LogError(ex, "Error in CompareDate");
                return false;
            }
        }

        /// <summary>
        /// Check string is valid timezone
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsValidTimeZone(string input)
        {
            try
            {
                if (String.IsNullOrEmpty(input))
                {
                    return true;
                }
                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(input);

                return true;
            }
            catch (TimeZoneNotFoundException)
            {
                TimeZoneInfo cstZone = null;

                try
                {
                    cstZone = TimeZoneInfo.FindSystemTimeZoneById(TZConvert.IanaToWindows(input));
                }
                catch
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Get from, to datetime for report
        /// </summary>
        /// <param name="fromToDate"></param>
        /// <param name="fromToTime"></param>
        /// <param name="isToDateTime"></param>
        /// <returns></returns>
        public static DateTime GetFromToDateTime(string fromToDate, string fromToTime, bool isToDateTime)
        {
            try
            {
                var formatDatetime =
                       ApplicationVariables.Configuration[
                           Constants.DateServerFormat + ":" + Thread.CurrentThread.CurrentCulture.Name];

                var dateTimeFromTo = fromToDate;
                if (DateTime.TryParse(fromToTime, out _))
                {
                    dateTimeFromTo += " " + fromToTime;
                    formatDatetime += " h:mm tt";
                }
                else
                {
                    if (!isToDateTime)
                    {
                        dateTimeFromTo += " 00:00:00";
                    }
                    else
                    {
                        dateTimeFromTo += " 23:59:59";
                    }
                    formatDatetime += " HH:mm:ss";
                }

                //formatDatetime = "mm.dd.yyyy hh:mm:ss";

                if(DateTime.TryParseExact(dateTimeFromTo, formatDatetime, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }
                else
                {
                    return DateTime.ParseExact(dateTimeFromTo, "MM.dd.yyyy h:mm tt", CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(Helpers));
                logger.LogError(ex, "Error in GetFromToDateTime");
                return DateTime.MinValue;
            }
        }

        public static DateTime GetFromToDateTimeConvert(string fromToDate, string fromToTime, bool isToDateTime)
        {
            try
            {
                var formatDatetime = "yyyy-MM-dd";

                var dateTimeFromTo = fromToDate;
                if (DateTime.TryParse(fromToTime, out _))
                {
                    dateTimeFromTo += " " + fromToTime;
                    formatDatetime += " HH:mm:ss";
                }
                else
                {
                    if (!isToDateTime)
                    {
                        dateTimeFromTo += " 00:00:00";
                    }
                    else
                    {
                        dateTimeFromTo += " 23:59:59";
                    }
                    formatDatetime += " HH:mm:ss";
                }

                //formatDatetime = "mm.dd.yyyy hh:mm:ss";
                return DateTime.ParseExact(dateTimeFromTo, formatDatetime,
                    CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(Helpers));
                logger.LogError(ex, "Error in GetFromToDateTimeConvert");
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Splitting a list or collection into chunks
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="locations"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static IEnumerable<List<T>> SplitList<T>(this List<T> locations, int size = Constants.Settings.MaxUserSendToIcu)
        {
            for (var i = 0; i < locations.Count; i += size)
            {
                yield return locations.GetRange(i, Math.Min(size, locations.Count - i));

            }
        }

        /// <summary>
        /// To demonstrate extraction of file extension from base64 string.
        /// </summary>
        /// <param name="base64String">base64 string.</param>
        /// <returns>Henceforth file extension from string.</returns>
        public static string GetFileExtension(string base64String)
        {
            try
            {
                if (!string.IsNullOrEmpty(base64String))
                {
                    if (base64String.StartsWith("data:"))
                    {
                        var regex = new Regex(@"/(.+?);");
                        var mc = regex.Matches(base64String);
                        return $".{mc[0].Groups[1].Value}";
                    }

                    var data = base64String.Substring(0, 5);
                    switch (data.ToUpper())
                    {
                        case "IVBOR":
                            return ".png";
                        case "/9J/4":
                            return ".jpg";
                        case "AAAAF":
                            return ".mp4";
                        case "JVBER":
                            return ".pdf";
                        case "AAABA":
                            return ".ico";
                        case "UMFYI":
                            return ".rar";
                        case "E1XYD":
                            return ".rtf";
                        case "U1PKC":
                            return ".txt";
                        case "MQOWM":
                        case "77U/M":
                            return ".srt";
                        default:
                            return string.Empty;
                    }
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Get string from value
        /// </summary>
        /// <param name="value"></param>
        public static string GetStringFromValueSetting(string value)
        {
            try
            {
                var list = JArray.Parse(value).ToList();
                return list[0].ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        public static int GetIntFromValueSetting(string value)
        {
            try
            {
                var list = JArray.Parse(value).ToList();
                return list.Count > 0 ? Convert.ToInt32(list[0]) : 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static bool GetBoolFromValueSetting(string value)
        {
            try
            {
                var list = JArray.Parse(value).ToList();
                return list.Count > 0 && Convert.ToBoolean(list[0]);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Check format for date
        /// </summary>
        /// <param name="strDate"></param>
        /// <returns></returns>
        public static bool DateFormatCheck(string strDate)
        {
            try
            {
                Convert.ToDateTime(strDate);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// Mask keypad
        /// </summary>
        /// <param name="keyPadPw"></param>
        /// <returns></returns>
        public static string MaskKeyPadPw(string keyPadPw)
        {
            if (string.IsNullOrEmpty(keyPadPw))
            {
                return string.Empty;
            }
            const int numShow = 1;
            var maskKeypad = keyPadPw.Substring(numShow, keyPadPw.Length - numShow);
            var visible = keyPadPw.Substring(0, numShow);
            return visible + RenderStar(maskKeypad);
        }

        /// <summary>
        /// Mask keypad
        /// </summary>
        /// <param name="keyPadPw"></param>
        /// <returns></returns>
        public static string MaskAndDecrytorKeyPadPw(string keyPadPw)
        {
            if (string.IsNullOrEmpty(keyPadPw))
            {
                return string.Empty;
            }

            var deKeyPad = Encryptor.Decrypt(keyPadPw,
                ApplicationVariables.Configuration[Constants.Settings.EncryptKey]);
            return MaskKeyPadPw(deKeyPad);
        }

        /// <summary>
        /// Render star
        /// </summary>
        /// <param name="pass"></param>
        /// <returns></returns>
        private static string RenderStar(string pass)
        {
            var result = string.Empty;
            foreach (var unused in pass)
            {
                result = result + "*";
            }
            return result;
        }

        public static string GetLocalTimeZone()
        {
            TimeZoneInfo localZone = TimeZoneInfo.Local;
            return localZone.Id;
        }

        // Convert Date Time to string by timezone
        public static string ConvertDateTimeToStringByTimeZone(DateTime datetime, string timeZone)
        {
            if (!String.IsNullOrEmpty(timeZone))
            {
                try
                {
                    TimeZoneInfo cstZone = timeZone.ToTimeZoneInfo();

                    DateTime cstTime = TimeZoneInfo.ConvertTime(datetime.ToUniversalTime(), cstZone);
                    return cstTime.ToString(Constants.DateTimeFormat.DdMMyyyyHHmmss);
                }
                catch (TimeZoneNotFoundException)
                {
                    return datetime.ToString(Constants.DateTimeFormat.DdMMyyyyHHmmss);
                }
            }
            return datetime.ToString(Constants.DateTimeFormat.DdMMyyyyHHmmss);
        }

        public static string EncryptSecretCode(string plainText)
        {
            byte[] encrypted;

            // SECURITY: Replaced RijndaelManaged with AES for better security
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(Constants.DynamicQr.Key);
                aesAlg.IV = Encoding.UTF8.GetBytes(ReverseString(Constants.DynamicQr.Key));
                aesAlg.Mode = CipherMode.CBC;

                // Create an encryptor to perform the stream transform
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption. 
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            // Return the encrypted bytes from the memory stream.
            var result = Combine(Encoding.UTF8.GetBytes(ReverseString(Constants.DynamicQr.Key)), encrypted);
            return BitConverter.ToString(result).Replace("-", "");
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        private static Random random = new Random();

        public static bool TryValidateWorkingType(string type)
        {
            if (String.IsNullOrEmpty(type))
            {
                return false;
            }

            String[] validDay = Constants.Attendance.ValidType.Split(',', ' ');
            if (!validDay.Contains(type))
            {
                return false;
            }
            return true;
        }

        public static bool TryValidateWorkingTime(string startDate, string endDate, string startTimeWorking = "00:00")
        {
            try
            {
                if(string.IsNullOrEmpty(startTimeWorking)){}
                {
                    startTimeWorking = "00:00";
                }
                if (String.IsNullOrEmpty(startDate) || String.IsNullOrEmpty(endDate))
                {
                    return false;
                }
                String[] strStartTimeWorking = startTimeWorking.Split(':', ' ');
                String[] strStart = startDate.Split(':', ' ');
                String[] strEnd = endDate.Split(':', ' ');
                var hourStartTimeWorking = Convert.ToInt32(strStartTimeWorking.FirstOrDefault());
                var hourStart = Convert.ToInt32(strStart.FirstOrDefault());
                var hourEnd = Convert.ToInt32(strEnd.FirstOrDefault());

                if (hourStartTimeWorking < 0 || hourStartTimeWorking > 24)
                {
                    return false;
                }

                if (hourStart < hourStartTimeWorking || hourStart > 24)
                {
                    return false;
                }

                if (hourEnd < 0 ||hourEnd > 24)
                {

                    return false;
                }

                var minutesStart = Convert.ToInt32(strStart[strStart.Length - 1]);
                var minutesEnd = Convert.ToInt32(strEnd[strEnd.Length - 1]);

                if (minutesStart < 0 || minutesStart > 60)
                {
                    return false;
                }

                if (minutesEnd < 0 || minutesEnd > 60)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(Helpers));
                logger.LogError(ex, "Error in TryValidateWorkingTime");
                return false;
            }
        }

        public static bool TryValidateWorkingDay(string day)
        {
            if (String.IsNullOrEmpty(day))
            {
                return false;
            }

            String[] validDay = Constants.Attendance.ValidDays.Split(',', ' ');
            if (!validDay.Contains(day))
            {
                return false;
            }
            return true;
        }

        public static bool TryValidateWorkingDayDistinct(List<string> listDay)
        {
            String[] validDay = Constants.Attendance.ValidDays.Split(',', ' ');

            if (listDay.Count != listDay.Distinct().Count())
            {
                // Duplicates exist
                return false;
            }

            if (listDay.Count != validDay.Length)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get date to write record attendance when receive event-log
        /// </summary>
        /// <param name="time">access time of event-log</param>
        /// <param name="startTimeWorking">start time working of event-log</param>
        /// <param name="start">start time of event-log</param>
        /// <param name="end">end time of event-log</param>
        /// <returns></returns>
        public static DateTime GetDateWriteAttendance(DateTime time, string startTimeWorking, string start, string end)
        {
            DateTime date = time.Date;
            if (string.IsNullOrEmpty(startTimeWorking))
                startTimeWorking = "00:00";
            // init date time start, end, startTimeWorking
            DateTime startD = date.Add(TimeSpan.Parse(start));
            DateTime endD = date.Add(TimeSpan.Parse(end));
            endD = ReFixEndDAttendance(startD, endD);
            DateTime startTimeWorkingD = startD.Add(TimeSpan.Parse(startTimeWorking) - TimeSpan.Parse(start));
            DateTime endTimeWorkingD = startTimeWorkingD.AddDays(1);
            
            if (startTimeWorkingD <= time && time <= endTimeWorkingD)
            {
                return startD.Date;
            }
            else if(time < startTimeWorkingD)
            {
                return date.AddDays(-1);
            }
            else if(endTimeWorkingD < time)
            {
                return date.AddDays(1);
            }
            
            return date;
        }
        
        public static DateTime ReFixEndDAttendance(DateTime startD, DateTime endD)
        {
            return startD >= endD ? endD.AddDays(1) : endD;
        }
        
        public static int GetUtcOffSetHour(string timeZone)
        {
            if (!String.IsNullOrEmpty(timeZone))
            {
                try
                {
                    TimeZoneInfo cstZone = timeZone.ToTimeZoneInfo();

                    var dt = DateTime.UtcNow;
                    var utcOffset = new DateTimeOffset(dt, TimeSpan.Zero);

                    var dateTimeOffset = utcOffset.ToOffset(cstZone.GetUtcOffset(utcOffset));
                    
                    return dateTimeOffset.Offset.Hours;

                    //var utc = cstZone.BaseUtcOffset.ToString().Split(":");
                    //var newUtc = "";
                    //if (utc[0].StartsWith("-"))
                    //{
                    //    newUtc = utc[0];
                    //}
                    //else
                    //{
                    //    newUtc = "+" + utc[0];
                    //}

                    //return Convert.ToInt32(newUtc);
                }
                catch (TimeZoneNotFoundException)
                {
                    return 0;
                }
            }

            return 0;
        }

        public static int GetUtcOffSetMinute(string timeZone)
        {
            if (!String.IsNullOrEmpty(timeZone))
            {
                try
                {
                    TimeZoneInfo cstZone = timeZone.ToTimeZoneInfo();

                    var dt = DateTime.UtcNow;
                    var utcOffset = new DateTimeOffset(dt, TimeSpan.Zero);

                    var dateTimeOffset = utcOffset.ToOffset(cstZone.GetUtcOffset(utcOffset));

                    return dateTimeOffset.Offset.Minutes;

                    //var utc = cstZone.BaseUtcOffset.ToString().Split(":");
                    //var newUtc = "";
                    //if (utc[0].StartsWith("-"))
                    //{
                    //    newUtc = "-" + utc[1];
                    //}
                    //else
                    //{
                    //    newUtc = "+" + utc[1];
                    //}
                    //return Convert.ToInt32(newUtc);
                }
                catch (TimeZoneNotFoundException)
                {
                    return 0;
                }
            }

            return 0;
        }

        public static string GenerateCompanyKey()
        {
            const string chars = Constants.DynamicQr.AllowChars;

            var secretCode = new string(Enumerable.Repeat(chars, Constants.DynamicQr.LenghtOfSecretCode)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return secretCode;
        }

        public static string GenerateSalt(int len)
        {
            if (len < Constants.DynamicQr.MaxByte)
            {
                const string chars = Constants.DynamicQr.Salt;
                var secretCode = new string(Enumerable.Repeat(chars, Constants.DynamicQr.MaxByte - len)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                return secretCode;   
            }

            return string.Empty;
        }

        public static string ReverseString(string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        public static TimeZoneInfo ToTimeZoneInfo(this string timeZone)
        {
            try
            {
                TimeZoneInfo cstZone = null;
                try
                {
                    cstZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                }
                catch
                {
                    cstZone = TimeZoneInfo.FindSystemTimeZoneById(TZConvert.IanaToWindows(timeZone));
                }

                return cstZone;
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.Local;
            }
        }

        public static int CheckStatusAttendance(double start, double end, double clockIn, double clockOut, bool checkClockOut = true, bool isRecheck = false)
        {
            if (checkClockOut)
            {
                if (clockIn > clockOut && clockOut != 0)
                {
                    return (short)AttendanceType.AbNormal;
                }
                else
                {
                    // Late 
                    if (clockIn > start + (double)59)
                    {
                        if (clockOut.Equals(0.0) || clockOut > end)
                        {
                            return (short)AttendanceType.LateIn;
                        }
                        else if (clockOut < end)
                        {
                            return (short)AttendanceType.LateInEarlyOut;
                        }

                    }

                    // Go Early
                    if (clockIn <= start + (double)59)
                    {
                        if (clockIn.Equals(0.0))
                        {
                            if (clockOut.Equals(0.0))
                            {
                                return (short)AttendanceType.AbsentNoReason;
                            }
                            else
                            {
                                return (short)AttendanceType.AbNormal;
                            }
                        }
                        else
                        {
                            if (clockOut.Equals(0.0))
                            {
                                return isRecheck ? (short)AttendanceType.AbNormal : (short)AttendanceType.Normal;
                            }
                            else
                            {
                                if (clockOut >= end)
                                {
                                    return (short)AttendanceType.Normal;
                                }
                                if (clockOut < end)
                                {
                                    return (short)AttendanceType.EarlyOut;
                                }
                            }
                        }
                    }
                }

                return (short)AttendanceType.AbsentNoReason;
            }
            else
            {
                // Late 
                if (clockIn > start + (double)59)
                {
                    return (short)AttendanceType.LateIn;
                }
                else
                {
                    if (clockIn.Equals(0.0))
                    {
                        return (short)AttendanceType.AbsentNoReason;
                    }
                    else
                    {
                        return (short)AttendanceType.Normal;
                    }
                }
            }
        }

        public static string GenerateRandomPasswordNumber(int len)
        {
            const string chars = Constants.DynamicQr.AllowCharsNumber;

            var secretCode = new string(Enumerable.Repeat(chars, len)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return secretCode;
        }

        public static DateTime UnixTimeToDateTime(double unixTime)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTime);
            return dtDateTime;
        }

        public static string ResizeImage(string textImage, int sizeX = 150, int sizeY = 150, bool useRotate = false)
        {
            // var tempRegex = @"data:image/(.*?);base64,";
            // var lsRegex = Regex.Matches(textImage, tempRegex, RegexOptions.Singleline);
            // if (lsRegex.Count > 0)
            //     textImage = textImage.Substring(lsRegex[0].Value.Length);
            try
            {
                var data = System.Convert.FromBase64String(textImage.FixBase64());
                var image = new MagickImage(data);

                if (useRotate)
                {
                    switch (image.Orientation)
                    {
                        case OrientationType.BottomRight:
                            image.Rotate(180);
                            break;
                        case OrientationType.RightTop:
                            image.Rotate(90);
                            break;
                        case OrientationType.LeftBotom:
                            image.Rotate(270);
                            break;
                    }
                }

                image.Resize(sizeX, sizeY);
                var output = image.ToByteArray();
                var outputBase64 = System.Convert.ToBase64String(output);

                return $"data:image/(.*?);base64,{outputBase64}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"===========================>{ex.Message}<===========================");
                Console.WriteLine(textImage);
                Console.WriteLine("===================================================================================");
                return "";
            }
        }

        public static string FixBase64(this string base64)
        {
            if (string.IsNullOrEmpty(base64)) return "";
            try
            {
                var tempRegex = @"data:image/(.*?);base64,";
                var lsRegex = Regex.Matches(base64, tempRegex, RegexOptions.Singleline);
                if (lsRegex.Count > 0)
                    base64 = base64.Substring(lsRegex[0].Value.Length);

                return base64.Trim();
            }
            catch (Exception)
            {
                return base64;
            }
        }

        public static bool IsTextBase64(this string text)
        {
            if (string.IsNullOrEmpty(text)) return false;

            try
            {
                text = text.FixBase64();
                Convert.FromBase64String(text);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public static string GetImageBase64FromUrl(string url)
        {
            string base64 = "";

            try
            {
                // Fixed base directory (e.g., "uploads" folder)
                string baseDir = Path.Combine(AppContext.BaseDirectory, Constants.Settings.DefineFolderImages);

                // If the url looks like a local file path
                if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    string pathLocal = FileHelpers.GetSecurePath(baseDir, url);

                    if (File.Exists(pathLocal))
                    {
                        string imageBase64 = Convert.ToBase64String(File.ReadAllBytes(pathLocal));
                        string extension = Path.GetExtension(pathLocal)?.TrimStart('.').ToLower() ?? "png";
                        base64 = $"data:image/{extension};base64,{imageBase64}";
                        return base64;
                    }
                }

                // If not local, process as an online URL
                using (WebClient webClient = new WebClient())
                {
                    byte[] dataArr = webClient.DownloadData(url);
                    string imageBase64 = Convert.ToBase64String(dataArr);

                    // Guess mime type from URL (basic, can be improved using HttpResponse.ContentType)
                    string extension = Path.GetExtension(url)?.TrimStart('.').ToLower() ?? "png";
                    base64 = $"data:image/{extension};base64,{imageBase64}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetImageBase64FromUrl] Error: {ex}");
                return "";
            }

            return base64;
        }

        public static string CheckPropertyInObject<T>(string name, string textDefault)
        {
            char[] names = name.ToCharArray();
            names[0] = char.ToUpper(names[0]);
            name = new string(names);
            PropertyInfo info = typeof(T).GetProperty(name);
            return info != null ? name : textDefault;
        }

        public static string CheckPropertyInObject<T>(string name, string textDefault, string[] columns)
        {
            if (int.TryParse(name, out var index))
                return columns != null ? columns[index] : textDefault;

            if (string.IsNullOrEmpty(name))
                return textDefault;

            char[] names = name.ToCharArray();
            names[0] = char.ToUpper(names[0]);
            name = new string(names);
            PropertyInfo info = typeof(T).GetProperty(name);
            return info != null ? name : textDefault;
        }

        public static string ReplaceSpacesString(this string str)
        {
            Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
            return regex.Replace(str, " ");
        }

        public static byte[] GetByteArrayApi(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    byte[] data = client.GetByteArrayAsync(url).Result;
                    return data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }
        
        public static JObject PostJson(string url, object obj = null, string token = null)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }

                    string json = obj != null ? JsonConvertCamelCase(obj) : "{}";
                    var dataBody = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = httpClient.PostAsync(url, dataBody);
                    response.Wait();
                    var result = response.Result;
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();
                    var data = readTask.Result;
                    if (result.IsSuccessStatusCode || !string.IsNullOrEmpty(data))
                    {
                        JObject stuff = JObject.Parse(data);
                        return stuff;   
                    }
                    else
                    {
                        var customResult = new { statusCode = result.StatusCode };
                        return JObject.Parse(JsonConvert.SerializeObject(customResult));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
        public static async Task<JObject> PostJsonAsync(string url, object obj = null, string token = null, int timeoutMs = 10000)
        {
            try
            {
                using (var httpClient = new HttpClient { Timeout = TimeSpan.FromMilliseconds(timeoutMs) })
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);
                    }

                    string json = obj != null ? JsonConvertCamelCase(obj) : "{}";
                    var dataBody = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(url, dataBody);
                    var data = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(data))
                    {
                        return JObject.Parse(data);
                    }
                    else
                    {
                        var customResult = new { statusCode = response.StatusCode, body = data };
                        return JObject.Parse(JsonConvert.SerializeObject(customResult));
                    }
                }
            }
            catch (TaskCanceledException) when (!System.Threading.CancellationToken.None.IsCancellationRequested)
            {
                // Timeout
                return JObject.Parse("{\"error\":\"timeout\"}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return JObject.Parse("{\"error\":\"exception\",\"message\":\"" + ex.Message + "\"}");
            }
        }

        public static string PostApiUsingBasicAuth(string url, object data, string username, string password)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    string json = data != null ? Helpers.JsonConvertCamelCase(data) : "{}";
                    var dataBody = new StringContent(json, Encoding.UTF8, "application/json");
                    var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    var response = httpClient.PostAsync(url, dataBody);
                    response.Wait();
                    var result = response.Result;
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();
                    return readTask.Result;
                }
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(Helpers));
                logger.LogError(ex, "Error in PostApiUsingBasicAuth");
                return null;
            }
        }

        public static JObject DeleteJson(string url, Dictionary<string, string> headers, string token)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                    
                    if (headers != null && headers.Any())
                    {
                        foreach (var header in headers)
                        {
                            httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                        }
                    }
                    
                    var response = httpClient.DeleteAsync(url);
                    response.Wait();
                    var result = response.Result;
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();
                    var data = readTask.Result;
                    JObject stuff = JObject.Parse(data);
                    return stuff;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
        
        public static string JsonConvertCamelCase(object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(Helpers));
                logger.LogError(ex, "Error in JsonConvertCamelCase");
                return null;
            }
        }

        public static async Task DeleteAccountRabbitMq(IConfiguration configuration, List<string> companiesCode, List<string> devicesAddress)
        {
            // var initial = new ManagementClient(
            //     configuration.GetSection("QueueConnectionSettingsApi:Host").Value,
            //     configuration.GetSection("QueueConnectionSettingsApi:UserName").Value,
            //     configuration.GetSection("QueueConnectionSettingsApi:Password").Value,
            //     Int32.Parse(configuration.GetSection("QueueConnectionSettingsApi:Port").Value)
            // );
            // var vHost = await initial.GetVhostAsync(configuration.GetSection("QueueConnectionSettingsApi:VirtualHost").Value);
            //
            // foreach (var userName in companiesCode)
            // {
            //     try
            //     {
            //         var user = await initial.GetUserAsync(userName);
            //         if (user != null)
            //             await initial.DeleteUserAsync(user);
            //     }
            //     catch
            //     {
            //         // ignored
            //         // user rabbit not existed
            //     }
            // }
            //
            // foreach (var userName in devicesAddress)
            // {
            //     try
            //     {
            //         var user = await initial.GetUserAsync(userName);
            //         if (user != null)
            //         {
            //             await initial.DeleteUserAsync(user);
            //             var deviceAddress = userName.Split("_")[1];
            //             var queue = await initial.GetQueueAsync(Constants.RabbitMq.MultipleMessagesTaskQueue + $"_{deviceAddress}", vHost);
            //             await initial.DeleteQueueAsync(queue);
            //         }
            //     }
            //     catch
            //     {
            //         // ignored
            //         // user rabbit not existed
            //     }
            // }
        }


        /// <summary>
        /// Remove all empty space
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string RemoveAllEmptySpace(this string source)
        {
            return String.Concat(source.Where(c => !Char.IsWhiteSpace(c)));
        }

        public static Dictionary<string, bool> GetSettingVisibleFields(string listFields, Type type, List<string> ignoredFields = null, ILogger logger = null)
        {
            Dictionary<string, bool> dic = new Dictionary<string, bool>();
            bool defaultAllowed = false;
            try
            {
                if (!string.IsNullOrEmpty(listFields))
                {
                    dic = JsonConvert.DeserializeObject<Dictionary<string, bool>>(listFields);
                }
                else
                {
                    defaultAllowed = true;
                }
            }
            catch (Exception ex)
            {
                defaultAllowed = true;
                if (logger != null)
                {
                    logger.LogError(ex.Message);
                }
            }

            foreach (var item in type.GetProperties())
            {
                string fieldName = char.ToLower(item.Name[0]) + item.Name.Substring(1);
                if (ignoredFields != null && ignoredFields.Contains(fieldName))
                {
                    if (dic.ContainsKey(fieldName))
                    {
                        dic.Remove(fieldName);
                    }
                    continue;
                }
                if (!dic.ContainsKey(fieldName))
                    dic.Add(fieldName, defaultAllowed);
            }

            return dic;
        }
        public static int GetMaxSplit(short deviceType)
        {
            int maxSplit = Constants.MaxSendIcuUserCount;

            switch (deviceType)
            {
                case (short)DeviceType.ITouchPop:
                case (short)DeviceType.ITouch30A:
                    maxSplit = Constants.MaxSendITouchPopUserCount;
                    break;
                case (short)DeviceType.ITouchPopX:
                case (short)DeviceType.Icu400:
                case (short)DeviceType.DQMiniPlus:
                    maxSplit = Constants.MaxSendPopXCount;
                    break;
                case (short)DeviceType.IT100:
                    maxSplit = Constants.MaxSendAndroidCount;
                    break;
                case (short)DeviceType.PM85:
                    maxSplit = Constants.MaxSendAndroidCount;
                    break;
                case (short)DeviceType.Icu300N:
                case (short)DeviceType.Icu300NX:
                    maxSplit = Constants.MaxSendIcuUserCount;
                    break;
            }

            return maxSplit;
        }

        public static int GetMaxFileSplit(short deviceType)
        {
            var maxSplitSize = Constants.Settings.MaxSizeSendToIcu;

            if (deviceType == (short)DeviceType.ITouchPop)
                maxSplitSize = 16 * Constants.Settings.MaxSizeSendToIcu;//65536
            else if (deviceType == (short)DeviceType.ITouchPopX)
                maxSplitSize = 8 * Constants.Settings.MaxSizeSendToIcu;//32768
            else if (deviceType == (short)DeviceType.DQMiniPlus)
                maxSplitSize = 8 * Constants.Settings.MaxSizeSendToIcu;//32768
            else if (deviceType == (short)DeviceType.Icu400)
                maxSplitSize = 8 * Constants.Settings.MaxSizeSendToIcu;//32768
            else if (deviceType == (short)DeviceType.ITouch30A)
                maxSplitSize = 16 * Constants.Settings.MaxSizeSendToIcu;//65536
            else if (deviceType == (short)DeviceType.IT100)
                maxSplitSize = 16 * Constants.Settings.MaxSizeSendToIcu;//65536
            else if (deviceType == (short)DeviceType.PM85)
                maxSplitSize = 16 * Constants.Settings.MaxSizeSendToIcu;//65536

            return maxSplitSize;
        }

        public static List<int> GetMatchedIdentificationType(short deviceType)
        {
            List<int> types = new List<int>();

            switch (deviceType)
            {
                case (short)DeviceType.Icu300N:
                case (short)DeviceType.Icu300NX:
                case (short)DeviceType.Icu400:
                    types.Add((int)CardType.NFC);
                    types.Add((int)CardType.PassCode);
                    types.Add((int)CardType.NFCPhone);
                    types.Add((int)CardType.QrCode);
                    types.Add((int)CardType.HFaceId);
                    types.Add((int)CardType.VehicleId);
                    types.Add((int)CardType.VehicleMotoBikeId);
                    break;
                case (short)DeviceType.ITouchPop:
                case (short)DeviceType.ITouchPopX:
                case (short)DeviceType.DQMiniPlus:
                case (short)DeviceType.PM85:
                    types.Add((int)CardType.NFC);
                    types.Add((int)CardType.PassCode);
                    types.Add((int)CardType.NFCPhone);
                    types.Add((int)CardType.QrCode);
                    types.Add((int)CardType.VehicleId);
                    types.Add((int)CardType.VehicleMotoBikeId);
                    break;
                case (short)DeviceType.IT100:
                    types.Add((int)CardType.NFC);
                    types.Add((int)CardType.NFCPhone);
                    types.Add((int)CardType.FaceId);
                    break;
                case (short)DeviceType.NexpaLPR:
                    types.Add((int)CardType.VehicleId);
                    types.Add((int)CardType.VehicleMotoBikeId);
                    break;
                case (short)DeviceType.FV6000:
                    types.Add((int)CardType.Vein);
                    types.Add((int)CardType.NFC);
                    types.Add((int)CardType.NFCPhone);
                    break;
                case (short)DeviceType.ITouch30A:
                case (short)DeviceType.DP636X:
                    types.Add((int)CardType.NFC);
                    types.Add((int)CardType.PassCode);
                    types.Add((int)CardType.QrCode);
                    types.Add((int)CardType.NFCPhone);
                    break;
                case (short)DeviceType.Biostation2:
                    types.Add((int)CardType.NFC);
                    // types.Add((int)CardType.NFCPhone);
                    types.Add((int)CardType.PassCode);
                    types.Add((int)CardType.FingerPrint);
                    break;
                case (short)DeviceType.DF970:
                    types.Add((int)CardType.NFC);
                    types.Add((int)CardType.QrCode);
                    types.Add((int)CardType.LFaceId);
                    types.Add((int)CardType.DCFaceId);
                    types.Add((int)CardType.VehicleId);
                    types.Add((int)CardType.VehicleMotoBikeId);
                    types.Add((int)CardType.VNID);
                    break;
                case (short)DeviceType.Icu500:
                    types.Add((int)CardType.NFC);
                    types.Add((int)CardType.QrCode);
                    types.Add((int)CardType.DCFaceId);
                    types.Add((int)CardType.VehicleId);
                    types.Add((int)CardType.VehicleMotoBikeId);
                    types.Add((int)CardType.VNID);
                    break;
                case (short)DeviceType.T2Face:
                    types.Add((int)CardType.NFC);
                    types.Add((int)CardType.QrCode);
                    types.Add((int)CardType.LFaceId);
                    types.Add((int)CardType.DCFaceId);
                    types.Add((int)CardType.VNID);
                    break;
                case (short)DeviceType.BA8300:
                    types.Add((int)CardType.NFC);
                    types.Add((int)CardType.NFCPhone);
                    types.Add((int)CardType.PassCode);
                    types.Add((int)CardType.QrCode);
                    types.Add((int)CardType.LFaceId);
                    types.Add((int)CardType.AratekFingerPrint);
                    types.Add((int)CardType.VNID);
                    break;
                case (short)DeviceType.RA08:
                    types.Add((int)CardType.NFC);
                    types.Add((int)CardType.QrCode);
                    types.Add((int)CardType.LFaceId);
                    types.Add((int)CardType.VNID);
                    break;
                case (short)DeviceType.DQ8500:
                    types.Add((int)CardType.NFC);
                    types.Add((int)CardType.QrCode);
                    types.Add((int)CardType.LFaceId);
                    types.Add((int)CardType.AratekFingerPrint);
                    types.Add((int)CardType.VNID);
                    break;
                case (short)DeviceType.DQ200:
                    types.Add((int)CardType.NFC);
                    types.Add((int)CardType.QrCode);
                    types.Add((int)CardType.LFaceId);
                    types.Add((int)CardType.VNID);
                    break;
                case (short)DeviceType.EbknReader:
                    types.Add((int)CardType.NFC);
                    types.Add((int)CardType.NFCPhone);
                    types.Add((int)CardType.EbknFaceId);
                    break;
                case (short)DeviceType.TBVision:
                    types.Add((int)CardType.NFC);
                    types.Add((int)CardType.TBFace);
                    break;
            }

            return types;
        }

        public static DateTime GetTimeStartVideoByEventTime(DateTime eventTime)
        {
            int secondDiv = eventTime.Second % 10;
            switch (secondDiv)
            {
                case 0:
                case 1:
                case 2:
                    return eventTime.AddSeconds(-5 - secondDiv);
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    return eventTime.AddSeconds(-secondDiv);
                default: // 8, 9
                    return eventTime.AddSeconds(-secondDiv + 5);
            }
        }

        /// <summary>
        /// This function returns max user count by device type.
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public static int GetMaximumUserCount(int deviceType)
        {
            switch (deviceType)
            {
                case (int)DeviceType.Icu300N:
                    return Constants.Settings.DefaultMaxIcuUser;
                default:
                    return Constants.Settings.DefaultMaxPopUser;
            }
        }

        /// <summary>
        /// Get file name from image file path.
        /// es. '~/static/images/h430861/avatar/6.jpg'  -> '6.jpg'
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileNameFromPath(this string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return "";

            var files = filePath.Split("/");

            return files.Last();
        }

        public static IQueryable<T> SortData<T>(IEnumerable<T> data, string sortDirection, string sortColumn)
        {
            // TEMP
            if (sortColumn.Equals("PlateNumberList")) return data.AsQueryable();

            PropertyDescriptor prop2 = TypeDescriptor.GetProperties(typeof(T)).Find(sortColumn, true);

            IOrderedEnumerable<T> result;

            if (sortDirection.ToLower().Equals("desc"))
            {
                if (prop2 == null)
                    result = data.OrderByDescending(m => m.GetType());
                else
                    result = data.OrderByDescending(m => prop2.GetValue(m));
            }
            else
            {
                if (prop2 == null)
                    result = data.OrderBy(m => m.GetType());
                else
                    result = data.OrderBy(m => prop2.GetValue(m)?.ToString(), new NullOrEmptyStringReducer());
            }

            return result.AsQueryable();
        }

        public static List<short> GetDisplayDevices()
        {
            List<short> devices = new List<short>()
            {
                (short) DeviceType.ITouchPop,
                (short) DeviceType.ITouchPopX,
                (short) DeviceType.DQMiniPlus,
                (short) DeviceType.IT100,
                (short) DeviceType.PM85,
                (short) DeviceType.ITouch30A,
            };

            return devices;
        }

        public static string GetIpAddressRequest(this HttpContext context)
        {
            string ipAddress = "";
            try
            {
                if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var xForwardedFor))
                {
                    ipAddress = xForwardedFor;
                }

                if (string.IsNullOrEmpty(ipAddress) && context.Request.Headers.TryGetValue("X-Real-IP", out var xRealIp))
                {
                    ipAddress = xRealIp;
                }

                if (string.IsNullOrEmpty(ipAddress) && context.Connection.RemoteIpAddress != null)
                {
                    ipAddress = context.Connection.RemoteIpAddress.MapToIPv4().ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return ipAddress;
        }

        public static string GetBackgroundColorByDoorStatus(int doorStatusId, out string fontColorCode)
        {
            string colorCode = "";

            Color backgroundDolor = Color.White;
            Color fontColor = Color.Black;

            switch (doorStatusId)
            {
                case (int)DoorStatus.ClosedAndLock:
                case (int)DoorStatus.ClosedAndUnlocked:
                case (int)DoorStatus.Opened:
                case (int)DoorStatus.PassageOpened:
                case (int)DoorStatus.Lock:
                case (int)DoorStatus.Unlock:
                case (int)DoorStatus.Inactive:
                case (int)DoorStatus.NoMealTime:
                case (int)DoorStatus.NoFire:
                    break;

                case (int) DoorStatus.ForceOpened:
                case (int) DoorStatus.EmergencyOpened:
                case (int) DoorStatus.NeedSetting:
                case (int) DoorStatus.Fire:
                    //colorCode = string.Format("0x{0:X8}", Color.Red.ToArgb());
                    //backgroundDolor = Color.Red;
                    backgroundDolor = Color.FromArgb(-2417893);
                    fontColor = Color.White;
                    break;

                case (int)DoorStatus.EmergencyClosed:
                case (int)DoorStatus.Invalid:
                    //colorCode = string.Format("0x{0:X8}", Color.DarkGray.ToArgb());
                    backgroundDolor = Color.DimGray;
                    fontColor = Color.White;
                    break;

                default:
                    break;
            }

            if(backgroundDolor != Color.White)
                colorCode = $"{backgroundDolor.R:X2}{backgroundDolor.G:X2}{backgroundDolor.B:X2}";

            fontColorCode = $"{fontColor.R:X2}{fontColor.G:X2}{fontColor.B:X2}";

            return colorCode;
        }


        /// <summary>
        /// This function to get valid card status list to send card data to device.
        /// </summary>
        /// <returns></returns>
        public static List<short> GetCardStatusToSend()
        {
            var cardStatus = new List<short>()
            {
                (short) CardStatus.Normal,
                (short) CardStatus.Transfer,
                (short) CardStatus.Retire,
                (short) CardStatus.Lost,
                (short) CardStatus.InValid,
            };

            return cardStatus;
        }

        public static string GenerateGroupMsgId()
        {
            string msgId = Guid.NewGuid().ToString();
            return msgId.Substring(0, msgId.Length - "-0000-0000".Length);
        }
        
        public static string CreateMsgIdProcess(string msgId, int index, int total, string noteMsg = "")
        {
            try
            {
                string newMsgId = msgId;
                if (!string.IsNullOrEmpty(noteMsg))
                {
                    newMsgId = $"{noteMsg}-{msgId}";
                }
                
                if (total == 0) return newMsgId;

                return $"{Constants.RabbitMq.PrefixMsgIdProcess}-{newMsgId}-{index:0000}-{total:0000}";
            }
            catch
            {
                return msgId;
            }
        }

        public static string CreateChangedValueContents(string objectName, object oldVal, object newVal)
        {
            string returnStr;
            string changeType = CommonResource.lblEdit;

            #region SET OLD VALUE
            string oldValue = string.Empty;
            if(oldVal == null)
            {
                oldValue = string.Empty;
            }
            else if (oldVal.GetType() == typeof(string))
            {
                oldValue = (string)oldVal;
            }
            else if(oldVal.GetType() == typeof(int))
            {
                oldValue = ((int)oldVal).ToString();
            }
            else if (oldVal.GetType() == typeof(int?))
            {
                oldValue = ((int?)oldVal)?.ToString();
            }

            if (string.IsNullOrWhiteSpace(oldValue))
            {
                changeType = CommonResource.btnAdd;
                oldValue = $"({CommonResource.None})";
            }
            #endregion

            #region SET NEW VALUE
            string newValue = string.Empty;
            if (newVal == null) 
            {
                newValue = string.Empty;
            } 
            else if (newVal.GetType() == typeof(string))
            {
                newValue = (string)newVal;
            }
            else if (newVal.GetType() == typeof(int))
            {
                newValue = ((int)newVal).ToString();
            }
            else if (newVal.GetType() == typeof(int?))
            {
                newValue = ((int?)newVal)?.ToString();
            }

            if (string.IsNullOrWhiteSpace(newValue))
            {
                changeType = CommonResource.Deleted;
                newValue = $"({CommonResource.None})";
            }
            #endregion

            // ⇾
            // →

            string changeContents = $" : {oldValue} ⇾ {newValue}";
            returnStr = $"[{changeType}] {objectName}{changeContents}";
            if(returnStr.Length > 20)
            {
                changeContents = changeContents.Replace(" :", "<br />:");

                if (changeContents.Length > 20) changeContents = changeContents.Replace("⇾", "<br /> ⇾");

                returnStr = $"[{changeType}] {objectName}{changeContents}";
            }

            //if (oldVal.GetType() == typeof(string))
            //{
            //    string oldValue = (string)oldVal;
            //    string newValue = (string)newVal;

            //    if (string.IsNullOrWhiteSpace(oldValue))
            //    {
            //        changeType = CommonResource.btnAdd;
            //        oldValue = "( - )";
            //    }

            //    if (string.IsNullOrWhiteSpace(newValue))
            //    {
            //        changeType = CommonResource.Deleted;
            //        newValue = "( - )";
            //    }

            //    returnStr = $"[{changeType}] {objectName}<br />: {oldValue}<br /> -> {newValue}";
            //}
            //else if (oldVal.GetType() == typeof(int))
            //{
            //    int oldValue = (int)oldVal;
            //    int newValue = (int)newVal;

            //    returnStr = $"[{changeType}] {objectName} : {oldValue} -> {newValue}";
            //}
            //else if (oldVal.GetType() == typeof(int?))
            //{
            //    int oldValue = (int?)oldVal ?? 0;
            //    int newValue = (int?)newVal ?? 0;

            //    string oldValueStr = oldValue.ToString();
            //    string newValueStr = newValue.ToString();

            //    if(oldValue == 0)
            //    {
            //        changeType = CommonResource.btnAdd;
            //        oldValueStr = "( - )";
            //    }

            //    if (newValue == 0)
            //    {
            //        changeType = CommonResource.Deleted;
            //        newValueStr = "( - )";
            //    }

            //    returnStr = $"[{changeType}] {objectName} : {oldValueStr} -> {newValueStr}";
            //}
            //else
            //{
            //    returnStr = $"[{changeType}] {objectName} : {oldVal} -> {newVal}";
            //}

            return returnStr;
        }

        public static bool HaveEmptySpaceInStr(this string textVal)
        {
            if (textVal.StartsWith(" ") || textVal.EndsWith(" "))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Gửi file và các trường form-data lên API dạng multipart/form-data
        /// </summary>
        public static async Task<bool> UploadFileMultipartFormData(string url, string fileName, byte[] fileData, object formFields, string token = null)
        {
            using (var client = new HttpClient())
            using (var content = new MultipartFormDataContent())
            {
                // Thêm file
                var fileContent = new ByteArrayContent(fileData);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                content.Add(fileContent, "file", fileName);

                // Thêm các trường form
                if (formFields != null)
                {
                    foreach (var kv in formFields.GetType().GetProperties())
                    {
                        var value = kv.GetValue(formFields)?.ToString() ?? "";
                        content.Add(new StringContent(value), kv.Name);
                    }
                }

                // Thêm token nếu có
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Upload file failed: {error}");
                }
                return true;
            }
        }
        
        /// <summary>
        /// Checks if a string appears to be source code or contains code-like patterns
        /// </summary>
        /// <param name="input">The string to check</param>
        /// <returns>True if the string looks like code, otherwise false</returns>
        public static bool ContainsCode(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var codePatterns = new[]
            {
                @"<[^>]+>",                    // HTML/XML tags
                @"\{.*\}",                     // Curly braces (JSON, code blocks)
                @"\[.*\]",                     // Square brackets (arrays, attributes)
                @"function\s*\(",              // JavaScript functions
                @"def\s+\w+\s*\(",             // Python functions
                @"public\s+\w+",               // C# public methods/properties
                @"private\s+\w+",              // C# private methods/properties
                @"class\s+\w+",                // Class definitions
                @"import\s+\w+",               // Import statements
                @"using\s+\w+",                // Using statements
                @"SELECT\s+.*\s+FROM",         // SQL queries
                @"INSERT\s+INTO",              // SQL inserts
                @"UPDATE\s+.*\s+SET",          // SQL updates
                @"DELETE\s+FROM",              // SQL deletes
                @"if\s*\(",                    // If statements
                @"for\s*\(",                   // For loops
                @"while\s*\(",                 // While loops
                @"catch\s*\(",                 // Try-catch blocks
                @"throw\s+new",                // Exception throwing
                @"return\s+\w+",               // Return statements
                @"var\s+\w+\s*=",             // Variable declarations
                @"let\s+\w+\s*=",             // JavaScript let declarations
                @"const\s+\w+\s*=",           // JavaScript const declarations
                @"console\.log\(",             // Console logging
                @"System\.out\.println\(",     // Java printing
                @"print\(",                    // Python print
                @"printf\(",                   // C printf
                @"<!--.*-->",                  // HTML comments
                @"//.*",                       // Single line comments
                @"/\*.*\*/",                   // Multi-line comments
                @"#.*",                        // Python/shell comments
                @"--.*",                       // SQL comments
                @"\$\{.*\}",                   // Template literals
                @"<%.*%>",                     // Server-side scripts
                @"<?.*?>",                     // PHP/XML processing instructions
            };

            foreach (var pattern in codePatterns)
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if a string contains only letters or digits.
        /// </summary>
        /// <param name="input">The string to check</param>
        /// <returns>True if the string contains only letters or digits</returns>
        public static bool ContainsOnlyLettersAndDigits(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false; // hoặc true nếu coi chuỗi rỗng là hợp lệ

            // Cho phép chữ cái (Unicode), số và khoảng trắng
            var safeCharPattern = @"^[\p{L}\p{Nd}\s]+$";
            return Regex.IsMatch(input, safeCharPattern);
        }
    }


    public static class KoreanHelpers
    {
        #region Field

        /// <summary>
        /// number of initial
        /// </summary>
        private const int INITIAL_COUNT = 19;

        /// <summary>
        /// number of medial
        /// </summary>
        private const int MEDIAL_COUNT = 21;

        /// <summary>
        /// number of fianl
        /// </summary>
        private const int FINAL_COUNT = 28;

        /// <summary>
        /// start index of Korean unicode
        /// </summary>
        private const int KOREAN_UNICODE_START_INDEX = 0xac00;

        /// <summary>
        /// end index of Korean unicode
        /// </summary>
        private const int KOREAN_UNICODE_END_INDEX = 0xD7A3;

        /// <summary>
        /// start index of initial
        /// </summary>
        private const int INITIAL_START_INDEX = 0x1100;

        /// <summary>
        /// start index of medial
        /// </summary>
        private const int MEDIAL_START_INDEX = 0x1161;

        /// <summary>
        /// start index of final
        /// </summary>
        private const int FINAL_START_INDEX = 0x11a7;

        private static readonly string[] startElement = { "ㄱ", "ㄲ", "ㄴ", "ㄷ", "ㄸ", "ㄹ", "ㅁ", "ㅂ", "ㅃ", "ㅅ", "ㅆ", "ㅇ", "ㅈ", "ㅉ", "ㅊ", "ㅋ", "ㅌ", "ㅍ", "ㅎ" };
        private static readonly string[] startElementEng = { "r", "R", "s", "e", "E", "f", "a", "q", "Q", "t", "T", "d", "w", "W", "c", "z", "x", "v", "g" };

        private static readonly string[] middleElement = { "ㅏ", "ㅐ", "ㅑ", "ㅒ", "ㅓ", "ㅔ", "ㅕ", "ㅖ", "ㅗ", "ㅘ", "ㅙ", "ㅚ", "ㅛ", "ㅜ", "ㅝ", "ㅞ", "ㅟ", "ㅠ", "ㅡ", "ㅢ", "ㅣ" };
        private static readonly string[] middleElementEng = { "k", "o", "i", "O", "j", "p", "u", "P", "h", "hk", "ho", "hl", "y", "n", "nj", "np", "nl", "b", "m", "ml", "l" };

        private static readonly string[] endElement = { " ", "ㄱ", "ㄲ", "ㄳ", "ㄴ", "ㄵ", "ㄶ", "ㄷ", "ㄹ", "ㄺ", "ㄻ", "ㄼ", "ㄽ", "ㄾ", "ㄿ", "ㅀ", "ㅁ", "ㅂ", "ㅄ", "ㅅ", "ㅆ", "ㅇ", "ㅈ", "ㅊ", "ㅋ", "ㅌ", "ㅍ", "ㅎ" };
        private static readonly string[] endElementEng = { "", "r", "R", "rt", "s", "sw", "sg", "e", "f", "fr", "fa", "fq", "ft", "fx", "fv", "fg", "a", "q", "qt", "t", "T", "d", "w", "c", "z", "x", "v", "g" };

        #endregion

        /// <summary>
        /// Check if the source is Korean
        /// </summary>
        /// <param name="source"> Source char </param>
        /// <returns> Whether the source is Korean or not </returns>
        public static bool IsKorean(char source)
        {
            if (KOREAN_UNICODE_START_INDEX <= source && source <= KOREAN_UNICODE_END_INDEX)
            {
                return true;
            }

            return false;
        }
        public static string[] DivideKorean(char source)
        {
            string[] elementArray = null;

            if (IsKorean(source))
            {
                int index = source - KOREAN_UNICODE_START_INDEX;

                int initial = index / (MEDIAL_COUNT * FINAL_COUNT);
                int medial = (index % (MEDIAL_COUNT * FINAL_COUNT)) / FINAL_COUNT;
                int final = index % FINAL_COUNT;

                if (final == 4519)
                {
                    elementArray = new string[2];

                    elementArray[0] = startElement[initial];
                    elementArray[1] = middleElement[medial];
                }
                else
                {
                    elementArray = new string[3];

                    elementArray[0] = startElement[initial];
                    elementArray[1] = middleElement[medial];
                    elementArray[2] = endElement[final];
                }
            }

            return elementArray;
        }

        /// <summary>
        /// Matching the source korean to English in keyborad rule
        /// </summary>
        /// <param name="korean"></param>
        /// <returns></returns>
        public static string[] ConvertKeyboardEng(string[] korean)
        {
            string[] elementArray = new string[korean.Length];

            var index = Array.IndexOf(startElement, korean[0]);

            elementArray[0] = startElementEng[Array.IndexOf(startElement, korean[0])];
            elementArray[1] = middleElementEng[Array.IndexOf(middleElement, korean[1])];

            if (korean.Length == 3)
            {
                elementArray[2] = endElementEng[Array.IndexOf(endElement, korean[2])];
            }

            return elementArray;
        }


        public static string EncodingUTF8OnlyKorean(string sourceStr)
        {
            string resultStr = "";
            foreach (char c in sourceStr.ToString())
            {
                if (KoreanHelpers.IsKorean(c))
                {
                    var utfStr = Encoding.UTF8.GetBytes(c.ToString());
                    foreach (var utfItem in utfStr)
                    {
                        resultStr += "%" + String.Format("{0:X}", utfItem);
                    }
                }
                else
                {
                    resultStr += c;
                }
            }

            return resultStr;
        }
    }
}
