using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Dir
{
    public static class PathHelper
    {
        public static bool TryGetParentPath(string path, out string parentPath)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            var tokens = path.Split(Path.DirectorySeparatorChar);

            if (tokens.Length == 1 || (tokens.Length == 2 && tokens.First() == ""))
            {
                parentPath = null;
                return false;
            }

            parentPath = string.Join(Path.DirectorySeparatorChar.ToString(), tokens.Take(tokens.Length - 1));
            return true;
        }

        public static string AppendDirectorySeparatorIfMissing(this string path)
        {
            if (path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return path;
            }

            return path + Path.DirectorySeparatorChar;
        }

        public static string GetShortDirectoryName(this string path)
        {
            return path.Substring(path.LastIndexOf('\\') + 1);
        }

        public static string GetFullPath(this FileSystemInfo file)
        {
            try
            {
                return file.FullName;
            }
            catch (PathTooLongException)
            {
                return (string)typeof(FileInfo)
                    .GetField("FullPath", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(file);
            }
        }
    }
}