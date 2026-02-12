using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RabbitMQ.Client;

namespace DeMasterProCloud.Service.Csv
{
    public class CsvHelper
    {
        public static void Write(string[] headers, List<string[]> contents, string csvFile)
        {
            using (var writer = File.AppendText(csvFile))
            {
                var csvBuilder = new StringBuilder();
                if (new FileInfo(csvFile).Length == 0 && headers != null)
                {
                    csvBuilder.AppendLine(string.Join(",", headers));
                }
                foreach (var content in contents)
                {
                    csvBuilder.AppendLine(string.Join(",", content));
                }
                writer.Write(csvBuilder.ToString());
            }
        }

        public static void BackupEventLog(string text, string folderBackup, string folderFile, string csvFile)
        {
            if (!File.Exists(folderBackup))
            {
                Directory.CreateDirectory(folderBackup);
            }
            
            if (!string.IsNullOrEmpty(folderFile) && !File.Exists($"{folderBackup}/{folderFile}"))
            {
                Directory.CreateDirectory($"{folderBackup}/{folderFile}");
            }

            csvFile = string.IsNullOrEmpty(folderFile) ? $"{folderBackup}/{csvFile}" : $"{folderBackup}/{folderFile}/{csvFile}";
            using (var writer = File.AppendText(csvFile))
            {
                writer.Write(text);
            }
        }
    }
}
