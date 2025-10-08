using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace DeMasterProCloud.Common.Infrastructure
{
    /// <summary>
    /// This class contain extension functions for string objects
    /// </summary>
    public static class StringExtensions
    {

        /// <summary>
        /// Uppercases the first character of a string
        /// </summary>
        /// <param name="input">The string which first character should be uppercased</param>
        /// <returns>The input string with it'input first character uppercased</returns>
        public static string FirstCharToUpper(this string input)
        {
            try
            {
                return string.IsNullOrEmpty(input)
                    ? ""
                    : string.Concat(input.Substring(0, 1).ToUpper(), input.Substring(1).ToLower());
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(StringExtensions));
                logger.LogError(ex, "Error in FirstCharToUpper");
                return string.Empty;
            }
        }

        /// <summary>
        /// Formats the string according to the specified mask
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static string Mask(this string input, char mask)
        {
            try
            {
                return string.IsNullOrEmpty(input) ? input : new string(mask, input.Length);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(StringExtensions));
                logger.LogError(ex, "Error in Mask");
                return string.Empty;
            }
        }

        /// <summary>
        /// Is numeric
        /// </summary>
        /// <param name="theValue"></param>
        /// <returns></returns>
        public static bool IsNumeric(this string theValue)
        {
            try
            {
                return long.TryParse(theValue, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out long _);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(StringExtensions));
                logger.LogError(ex, "Error in IsNumeric");
                return false;
            }
        }

        /// <summary>
        /// Convert a string to camel case
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToCamelCase(this string str)
        {
            try
            {
                if (!string.IsNullOrEmpty(str) && str.Length > 1)
                {
                    return char.ToLowerInvariant(str[0]) + str.Substring(1);
                }
                return str;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(StringExtensions));
                logger.LogError(ex, "Error in ToCamelCase");
                return string.Empty;
            }
        }

        public static string ToPascalCase(this string self)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(self) || char.IsUpper(self[0])) return self;
                return char.ToUpper(self[0]) + self.Substring(1);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(StringExtensions));
                logger.LogError(ex, "Error in ToPascalCase");
                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="base64String"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
        public static List<string> SplitString(this string base64String, int chunkSize)
        {
            try
            {
                int size = base64String.Length / chunkSize;
                var nineFirst = Enumerable.Range(0, chunkSize - 1)
                    .Select(i => base64String.Substring(i * size, size)).ToList();
                var firstLength = nineFirst.Count * size;
                var last =
                    base64String.Substring(firstLength, base64String.Length - firstLength);
                nineFirst.Add(last);
                return nineFirst;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(StringExtensions));
                logger.LogError(ex, "Error in SplitString");
                return new List<string>();
            }
        }
        
        public static string Base64Encode(this string text)
        {
            try
            {
                var data = Encoding.UTF8.GetBytes(text);
                return System.Convert.ToBase64String(data);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(StringExtensions));
                logger.LogError(ex, "Error in Base64Encode");
                return string.Empty;
            }
        }

        public static string Base64Decode(this string base64)
        {
            try
            {
                var data = System.Convert.FromBase64String(base64);
                return Encoding.UTF8.GetString(data);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(StringExtensions));
                logger.LogError(ex, "Error in Base64Decode");
                return string.Empty;
            }
            
        }
    }
}