using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DeMasterProCloud.Common.Infrastructure
{
    public static class FileHelpers
    {
        /// <summary>
        /// Convert from file to base 64
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static MemoryStream ConvertToStream(IFormFile file)
        {
            try
            {
                var ms = new MemoryStream();
                file.CopyTo(ms);
                return ms;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(FileHelpers));
                logger.LogError(ex, "Error in ConvertToStream");
                return null;
            }
        }

        /// <summary>
        /// Convert from file to string array
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string[] ConvertToStringArray(IFormFile file)
        {
            try
            {
                string result;
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    result = reader.ReadToEnd();
                }

                return string.IsNullOrEmpty(result)
                    ? null
                    : result.Split('\n');
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(FileHelpers));
                logger.LogError(ex, "Error in ConvertToStringArray");
                return null;
            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            try
            {
                StringBuilder hex = new StringBuilder(ba.Length * 2);
                foreach (byte b in ba)
                    hex.AppendFormat("{0:x2}", b);
                return hex.ToString();
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(FileHelpers));
                logger.LogError(ex, "Error in ByteArrayToString");
                return string.Empty;
            }
        }

        /// <summary>
        /// Get file extension from file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFileExtension(IFormFile file)
        {
            try
            {
                var fileExtension = "";
                var fileName = file.FileName;
                if (fileName.Contains("."))
                {
                    fileExtension = "." + fileName.Split(".").Last();
                }

                return fileExtension;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(FileHelpers));
                logger.LogError(ex, "Error in GetFileExtension");
                return string.Empty;
            }
        }
        public static string GetFileExtensionByFileName(string fileName)
        {
            try
            {
                var fileExtension = "";
                if (fileName.Contains("."))
                {
                    fileExtension = "." + fileName.Split(".").Last();
                }

                return fileExtension;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(FileHelpers));
                logger.LogError(ex, "Error in GetFileExtensionByFileName");
                return string.Empty;
            }
        }

        /// <summary>
        /// Get file name without extension and fullpath
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFileNameWithoutExtension(IFormFile file)
        {
            try
            {
                //var fileExtension = "";
                var fileName = file.FileName;

                if (fileName.Contains("\\"))
                {
                    fileName = fileName.Split("\\").Last();
                }

                if (fileName.Contains("."))
                {
                    var fileExtension = "." + fileName.Split(".").Last();
                    fileName = fileName.Substring(0, fileName.Length - fileExtension.Length);
                }

                return fileName;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(FileHelpers));
                logger.LogError(ex, "Error in GetFileNameWithoutExtension");
                return string.Empty;
            }
        }

        /// <summary>
        /// Get file name without extension and fullpath
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFileNameWithoutExtensionByFileName(string fileName)
        {
            try
            {
                if (fileName.Contains("\\"))
                {
                    fileName = fileName.Split("\\").Last();
                }

                if (fileName.Contains("."))
                {
                    var fileExtension = "." + fileName.Split(".").Last();
                    fileName = fileName.Substring(0, fileName.Length - fileExtension.Length);
                }

                return fileName;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(FileHelpers));
                logger.LogError(ex, "Error in GetFileNameWithoutExtensionByFileName");
                return string.Empty;
            }
        }

        /// <summary>
        /// Split file return list hex
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
        public static List<string> SplitFile(IFormFile inputFile, int chunkSize)
        {
            try
            {
                var buffer = new byte[chunkSize];
                var listHex = new List<string>();
                using (var input = inputFile.OpenReadStream())
                {
                    while (input.Position < input.Length)
                    {
                        using (var ms = new MemoryStream())
                        {
                            int remaining = chunkSize, bytesRead;
                            while (remaining > 0 && (bytesRead = input.Read(buffer, 0,
                                       Math.Min(remaining, chunkSize))) > 0)
                            {
                                ms.Write(buffer, 0, bytesRead);
                                remaining -= bytesRead;
                            }
                            var fileBytes = ms.ToArray();
                            var hexString = ByteArrayToString(fileBytes);
                            listHex.Add(hexString);
                        }
                    }
                }

                return listHex;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(FileHelpers));
                logger.LogError(ex, "Error in SplitFile");
                return new List<string>();
            }
        }

        /// <summary>
        /// Split file byte array and return list hex
        /// </summary>
        /// <param name="fileBytes"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
        public static List<string> SplitFileByByteArray(byte[] fileBytes, int chunkSize)
        {
            try
            {
                var listHex = new List<string>();
                using (var ms = new MemoryStream(fileBytes))
                {
                    var buffer = new byte[chunkSize];
                    int bytesRead;

                    while ((bytesRead = ms.Read(buffer, 0, chunkSize)) > 0)
                    {
                        var chunkBytes = new byte[bytesRead];
                        Array.Copy(buffer, chunkBytes, bytesRead);

                        var hexString = ByteArrayToString(chunkBytes);
                        listHex.Add(hexString);
                    }
                }

                return listHex;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(FileHelpers));
                logger.LogError(ex, "Error in SplitFileByByteArray");
                return new List<string>();
            }
        }

        /// <summary>
        /// Get lastest fileName
        /// </summary>
        /// <param name="inputDirectoryPath"></param>
        /// <param name="deviceAddress"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetLastestFileName(string inputDirectoryPath, string deviceAddress, string type)
        {
            try
            {
                var reg = $"{Constants.ExportType.LoadAllUser}_{deviceAddress}";
                var directoryInfo = new DirectoryInfo(inputDirectoryPath);
                var files = directoryInfo.GetFiles().Where(f => f.Extension == ".csv" && f.Name.Contains(reg)).ToArray();

                var fileResult = files.OrderByDescending(x => x.CreationTime).FirstOrDefault();
                return fileResult != null ? fileResult.Name : string.Empty;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(FileHelpers));
                logger.LogError(ex, "Error in GetLastestFileName");
                return string.Empty;
            }
        }

        private static bool SaveFileImage(string base64, string path, int maxSize = 0, int rotate = 0)
        {
            string[] folders = path.Split("/");
            string countFolder = folders[0];
            if (!Directory.Exists(countFolder))
            {
                Directory.CreateDirectory(countFolder);
            }
            for (int i = 1; i < folders.Length - 1; i++)
            {
                countFolder += $"/{folders[i]}";
                if (!Directory.Exists(countFolder))
                {
                    Directory.CreateDirectory(countFolder);
                }
            }
            byte[] data = Convert.FromBase64String(base64.FixBase64());
            MagickImage image = new MagickImage(new MemoryStream(data));
            using (MemoryStream ms = new MemoryStream(data))
            {
                try
                {
                    if (rotate != 0)
                    {
                        image.Rotate(rotate);
                    }

                    if (maxSize != 0 && data.Length > maxSize)
                    {
                        var resize = Math.Sqrt(data.Length / maxSize);
                        if (resize >= 1)
                        {
                            int newWidth = (int)(image.Width / resize);
                            int newHeight = (int)(image.Height / resize);
                            image.Resize(newWidth, newHeight);
                        }
                    }

                    var output = image.ToByteArray();
                    File.WriteAllBytes(path, output);
                    ms.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine("[Exception Save Image]: " + e.Message);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Secure version of SaveFileImage that validates path parameters to prevent path traversal attacks
        /// </summary>
        public static bool SaveFileImageSecure(string base64, string basePath, string fileName, int maxSize = 0, int rotate = 0)
        {
            // Validate fileName for security
            if (!IsValidPathParameter(fileName))
            {
                return false;
            }

            // Get secure path
            var securePath = GetSecurePath(basePath, fileName);
            if (securePath == null)
            {
                return false;
            }

            // Create directory structure for absolute path
            string directory = Path.GetDirectoryName(securePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch (Exception e)
                {
                    Console.WriteLine("[Exception Creating Directory]: " + e.Message);
                    return false;
                }
            }
            else
            {
                Console.WriteLine("[SaveFileImageSecure] Directory already exists or is empty");
            }

            // Process and save the image
            try
            {
                byte[] data = Convert.FromBase64String(base64.FixBase64());
                MagickImage image = new MagickImage(new MemoryStream(data));
                using (MemoryStream ms = new MemoryStream(data))
                {
                    if (rotate != 0)
                    {
                        image.Rotate(rotate);
                    }

                    if (maxSize != 0 && data.Length > maxSize)
                    {
                        var resize = Math.Sqrt(data.Length / maxSize);
                        if (resize >= 1)
                        {
                            int newWidth = (int)(image.Width / resize);
                            int newHeight = (int)(image.Height / resize);
                            image.Resize(newWidth, newHeight);
                        }
                    }

                    var output = image.ToByteArray();
                    File.WriteAllBytes(securePath, output);
                    ms.Dispose();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[Exception Save Image Secure]: " + e.Message);
                return false;
            }

            Console.WriteLine("[SaveFileImageSecure] Successfully completed");
            return true;
        }
        public static void DeleteFileFromLink(string link)
        {
            if (string.IsNullOrWhiteSpace(link))
                return;

            try
            {
                // Validate that the path doesn't contain path traversal attempts
                if (link.Contains("..") || link.Contains("~"))
                {
                    throw new UnauthorizedAccessException("Invalid file path detected");
                }

                // Normalize path separators
                string cleanPath = link.Replace('\\', '/');

                // Ensure the path starts with the allowed base directory
                string baseDirectory = Constants.Settings.DefineFolderImages;
                if (!cleanPath.StartsWith(baseDirectory + "/") && !cleanPath.Equals(baseDirectory))
                {
                    throw new UnauthorizedAccessException("Access denied to path outside allowed directory");
                }

                // Convert to full path for file operations
                string fullPath = Path.GetFullPath(cleanPath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex) when (!(ex is UnauthorizedAccessException))
            {
                // Log the error but don't expose internal paths
                Console.WriteLine($"Error deleting file: {ex.Message}");
            }
        }

        /// <summary>
        /// Securely saves an IFormFile to a specified base directory with path traversal protection
        /// </summary>
        /// <param name="file">The file to save</param>
        /// <param name="basePath">The base directory where the file should be saved</param>
        /// <param name="fileName">The desired filename (will be validated)</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool SaveFileByIFormFileSecure(IFormFile file, string basePath, string fileName)
        {
            try
            {
                // Validate the filename for path traversal
                if (!IsValidPathParameter(fileName))
                {
                    Console.WriteLine($"[Security] Invalid filename rejected: {fileName}");
                    return false;
                }

                // Construct secure path
                var securePath = GetSecurePath(basePath, fileName);
                if (securePath == null)
                {
                    Console.WriteLine($"[Security] Path traversal attempt blocked: {basePath}/{fileName}");
                    return false;
                }

                // Ensure the base directory exists
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                using (Stream fileStream = new FileStream(securePath, FileMode.Create, FileAccess.Write))
                {
                    file.CopyTo(fileStream);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to save file: {ex.Message}");
                return false;
            }
        }

        public static bool SaveFileByBytes(byte[] bytes, string path)
        {
            try
            {
                // Validate that the path doesn't contain path traversal attempts
                if (path.Contains("..") || path.Contains("~"))
                {
                    throw new UnauthorizedAccessException("Invalid file path detected");
                }

                // Normalize path separators
                string cleanPath = path.Replace('\\', '/');

                // Ensure the path starts with allowed base directories
                string[] allowedBasePaths = {
                    Constants.Settings.DefineFolderImages ?? "images",
                    Constants.Settings.DefineFolderVideos ?? "videos",
                    Constants.Settings.DefineFolderAttendance ?? "attendance"
                };

                bool isAllowed = false;
                foreach (string baseDir in allowedBasePaths)
                {
                    if (cleanPath.StartsWith(baseDir + "/") || cleanPath.Equals(baseDir))
                    {
                        isAllowed = true;
                        break;
                    }
                }

                if (!isAllowed)
                {
                    throw new UnauthorizedAccessException("Access denied to path outside allowed directories");
                }

                // Create directory structure securely
                string[] folders = cleanPath.Split("/");
                string countFolder = folders[0];
                if (!Directory.Exists(countFolder))
                {
                    Directory.CreateDirectory(countFolder);
                }
                for (int i = 1; i < folders.Length - 1; i++)
                {
                    countFolder += $"/{folders[i]}";
                    if (!Directory.Exists(countFolder))
                    {
                        Directory.CreateDirectory(countFolder);
                    }
                }

                File.WriteAllBytes(cleanPath, bytes);
                return true;
            }
            catch (Exception e) when (!(e is UnauthorizedAccessException))
            {
                Console.WriteLine($"[Exception save file error]: {e.Message} {e.StackTrace}");
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                // Log security violation but don't expose details
                Console.WriteLine("Security violation: Attempted path traversal attack blocked");
                return false;
            }
        }

        public static void CleanFileAppSetting(string environmentName)
        {
            // string rootFolder = "/mnt/16A4D878A4D85C35/code/dmpw-api/API/DeMasterProCloud.Api";
            string rootFolder = "/app";
            string pattern = "*appsettings*json";
            try
            {
                var files = System.IO.Directory.GetFiles(rootFolder, pattern);
                foreach (var file in files)
                {
                    if (!(file.Contains($"appsettings.{environmentName}.json") || file.Contains("appsettings.json")))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException?.Message);
            }
        }

        private static string GetExtensionOfFile(string pathFile)
        {
            try
            {
                return pathFile.Split('.').Last();
            }
            catch
            {
                return "";
            }
        }

        public static string GetContentTypeByFileName(string fileName)
        {
            try
            {
                string extension = GetExtensionOfFile(fileName);
                switch (extension)
                {
                    case "txt": return "application/text";
                    case "csv": return "application/csv";
                    case "pdf": return "application/pdf";
                    case "zip": return "application/zip";
                    case "xlsx": return "application/ms-excel";
                    case "mp4": return "video/mp4";
                    case "avi": return "video/avi";
                    case "webm": return "video/webm";
                    case "jpg": return "image/jpg";
                    case "jpeg": return "image/jpeg";
                    case "png": return "image/png";

                    default: return "application/text";
                }
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(FileHelpers));
                logger.LogError(ex, "Error in GetContentTypeByFileName");
                return "application/text";
            }
        }

        public static void AddTextToFile(string path, string content)
        {
            try
            {
                string[] folders = path.Split("/");
                string countFolder = folders[0];
                if (!Directory.Exists(countFolder))
                {
                    Directory.CreateDirectory(countFolder);
                }
                for (int i = 1; i < folders.Length - 1; i++)
                {
                    countFolder += $"/{folders[i]}";
                    if (!Directory.Exists(countFolder))
                    {
                        Directory.CreateDirectory(countFolder);
                    }
                }

                if (!File.Exists(path))
                {
                    File.WriteAllText(path, "");
                }

                File.AppendAllText(path, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Validates path parameters to prevent path traversal attacks
        /// </summary>
        /// <param name="parameter">The path parameter to validate</param>
        /// <returns>True if the parameter is safe, false if it contains malicious patterns</returns>
        public static bool IsValidPathParameter(string parameter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(parameter))
                {
                    Console.WriteLine("[IsValidPathParameter] Parameter is null or whitespace");
                    return false;
                }

                if (parameter.Contains(".."))
                {
                    Console.WriteLine("[IsValidPathParameter] Contains '..' - rejected");
                    return false;
                }
                if (parameter.Contains("\\"))
                {
                    Console.WriteLine("[IsValidPathParameter] Contains '\\' - rejected");
                    return false;
                }
                if (parameter.StartsWith("/"))
                {
                    Console.WriteLine("[IsValidPathParameter] Starts with '/' - rejected");
                    return false;
                }
                if (parameter.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                {
                    Console.WriteLine("[IsValidPathParameter] Contains invalid path chars - rejected");
                    return false;
                }
                if (parameter.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    Console.WriteLine("[IsValidPathParameter] Contains invalid filename chars - rejected");
                    return false;
                }

                Console.WriteLine("[IsValidPathParameter] Parameter is valid");
                return true;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(FileHelpers));
                logger.LogError(ex, "Error in IsValidPathParameter");
                return false;
            }
        }

        /// <summary>
        /// Safely constructs and validates a file path to prevent path traversal attacks
        /// </summary>
        /// <param name="basePath">The base directory path</param>
        /// <param name="pathSegments">The path segments to combine</param>
        /// <returns>The validated full path, or null if the path is unsafe</returns>
        public static string GetSecurePath(string basePath, params string[] pathSegments)
        {
            try
            {
                // Validate all path segments
                foreach (string segment in pathSegments)
                {
                    if (!IsValidPathParameter(segment))
                    {
                        return null;
                    }
                }

                // Construct the full path
                string[] allSegments = new string[pathSegments.Length + 1];
                allSegments[0] = basePath;
                Array.Copy(pathSegments, 0, allSegments, 1, pathSegments.Length);

                string fullPath = Path.GetFullPath(Path.Combine(allSegments));
                string normalizedBasePath = Path.GetFullPath(basePath);

                // Ensure the resolved path is within the base directory
                if (!fullPath.StartsWith(normalizedBasePath + Path.DirectorySeparatorChar) &&
                    !fullPath.Equals(normalizedBasePath))
                {
                    return null;
                }

                return fullPath;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[GetSecurePath] Exception: {e.Message}");
                return null;
            }
        }
    }
}
