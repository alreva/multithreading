using System.IO;
using System.Linq;

namespace Dir.Display
{
    public static class PathHelper
    {
        public static string GetShortDirectoryName(this string path)
        {
            return path
                .Split(Path.DirectorySeparatorChar)
                .Reverse()
                .SkipWhile(string.IsNullOrWhiteSpace)
                .FirstOrDefault();
        }

        public static bool TryGetParentPath(string path, out string parentPath)
        {
            if (path == null)
            {
                parentPath = null;
                return false;
            }

            string[] tokens = path.Split(Path.DirectorySeparatorChar);

            if (IsFileSystemRoot(tokens) || IsNetworkFolderRoot(tokens))
            {
                parentPath = null;
                return false;
            }

            parentPath = string.Join(Path.DirectorySeparatorChar.ToString(), tokens.Take(tokens.Length - 1));
            return true;
        }

        private static bool IsNetworkFolderRoot(string[] tokens)
        {
            return tokens.Length == 2 && tokens.First() == "";
        }

        private static bool IsFileSystemRoot(string[] tokens)
        {
            return tokens.Count(t => !string.IsNullOrWhiteSpace(t)) == 1;
        }
    }
}