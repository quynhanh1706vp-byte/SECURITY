using System;
using System.IO;
using Newtonsoft.Json;

namespace DeMasterProCloud.Common.Infrastructure
{
    public static class AppLog
    {
        public static void Init()
        {
            var dirPath = "AppLog";
            var directoryInfo = new DirectoryInfo(dirPath);
            if (directoryInfo.Exists == false)
            {
                directoryInfo.Create();
            }

            directoryInfo = new DirectoryInfo(dirPath + "/" + "Event");

            if (directoryInfo.Exists == false)
            {
                directoryInfo.Create();
            }

            directoryInfo = new DirectoryInfo(dirPath + "/" + "EventFailed");

            if (directoryInfo.Exists == false)
            {
                directoryInfo.Create();
            }
        }

        public static void EventSave(string eventLog)
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;
            var day = DateTime.Now.Day;

            var dirPath = $"AppLog/Event/{year}";
            var directoryInfo = new DirectoryInfo(dirPath);
            if (directoryInfo.Exists == false)
            {
                directoryInfo.Create();
            }

            dirPath += string.Format("/{0:00}", month);
            directoryInfo = new DirectoryInfo(dirPath);

            if (directoryInfo.Exists == false)
            {
                directoryInfo.Create();
            }

            var fulPath = dirPath + "/" + day + ".txt";
            using (StreamWriter outputFile = new StreamWriter(fulPath, true))
            {
                outputFile.WriteLine(eventLog);
            }
        }

        public static void EventFailedSave(byte[] data)
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;
            var day = DateTime.Now.Day;

            var dirPath = $"AppLog/EventFailed/{year}";
            var directoryInfo = new DirectoryInfo(dirPath);
            if (directoryInfo.Exists == false)
            {
                directoryInfo.Create();
            }

            dirPath += string.Format("/{0:00}", month);
            directoryInfo = new DirectoryInfo(dirPath);

            if (directoryInfo.Exists == false)
            {
                directoryInfo.Create();
            }

            var fulPath = dirPath + "/" + day + ".txt";
            using (StreamWriter outputFile = new StreamWriter(fulPath, true))
            {
                //System.Text.Encoding.Default.GetString(data);
                outputFile.WriteLine(System.Text.Encoding.Default.GetString(data));
            }
        }
    }
}
