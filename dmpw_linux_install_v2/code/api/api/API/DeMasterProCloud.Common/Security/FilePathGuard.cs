using System;
using System.IO;

namespace DeMasterProCloud.Common.Security
{
    public static class FilePathGuard
    {
        public static string EnsureUnderBaseDir(string baseDir, string userPath)
        {
            var fullBase = Path.GetFullPath(baseDir).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                           + Path.DirectorySeparatorChar;
            var fullUser = Path.GetFullPath(Path.Combine(baseDir, userPath ?? string.Empty));
            if (!fullUser.StartsWith(fullBase, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Path escapes base directory.");
            return fullUser;
        }

        public static string FilenameOnly(string userFilename)
            => Path.GetFileName(userFilename ?? string.Empty);
    }
}
