using System;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace DeMasterProCloud.Common.Infrastructure
{
    public class JsonSerializer : ISerializer
    {
        /// <summary>
        /// Serialize an object to byte array
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public byte[] Serialize(object obj)
        {
            try
            {
                if (obj == null)
                {
                    return null;
                }

                var json = JsonConvert.SerializeObject(obj);
                return Encoding.UTF8.GetBytes(json);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(JsonSerializer));
                logger.LogError(ex, "Error in Serialize");
                return null;
            }
        }

        /// <summary>
        /// DeSerialize byte array to an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arrBytes"></param>
        /// <returns></returns>
        public T DeSerialize<T>(byte[] arrBytes)
        {
            try
            {
                var json = Encoding.UTF8.GetString(arrBytes);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(JsonSerializer));
                logger.LogError(ex, "Error in DeSerialize");
                return default(T);
            }
        }
    }
}
