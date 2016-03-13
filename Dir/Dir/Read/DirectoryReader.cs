using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;

namespace Dir.Read
{
    public class DirectoryReader
    {
        public event EventHandler<string> DirectoryDiscovered;
        public event EventHandler<IEnumerable<FileSystemNode>> FilesRead;
        public event EventHandler<FileSystemNode> DirectoryRead;
        public event EventHandler<string> SecurityError;

        public void Run(string path)
        {
            LoadDirectoryInternal(path);
        }

        private long LoadDirectoryInternal(string path)
        {
            DirectoryInfo directory;
            try
            {
                directory = new DirectoryInfo(path);
            }
            catch (SecurityException)
            {
                OnSecurityError(path);
                return 0;
            }

            string[] subDirectoryPaths;
            try
            {
                subDirectoryPaths = Directory.GetDirectories(path);
            }
            catch (UnauthorizedAccessException e)
            {
                OnSecurityError(path);
                return 0;
            }

            foreach (string subDirectoryPath in subDirectoryPaths)
            {
                OnDirectoryDiscovered(subDirectoryPath);
            }

            FileInfo[] files = directory.GetFiles();

            OnFilesRead(files.Select(file => new FileSystemNode(GetFullPath(file), file.Length)));

            long fullSize = files.Sum(f => f.Length);

            foreach (string subDirectoryPath in subDirectoryPaths)
            {
                long subDirectorySize = LoadDirectoryInternal(subDirectoryPath);
                fullSize += subDirectorySize;
            }

            OnDirectoryRead(new FileSystemNode(GetFullPath(directory), fullSize));

            return fullSize;
        }

        protected virtual void OnDirectoryDiscovered(string e)
        {
            DirectoryDiscovered?.Invoke(this, e);
        }

        protected virtual void OnFilesRead(IEnumerable<FileSystemNode> e)
        {
            FilesRead?.Invoke(this, e);
        }

        protected virtual void OnDirectoryRead(FileSystemNode e)
        {
            DirectoryRead?.Invoke(this, e);
        }

        protected virtual void OnSecurityError(string e)
        {
            SecurityError?.Invoke(this, e);
        }

        public static string GetFullPath(FileSystemInfo info)
        {
            try
            {
                return info.FullName;
            }
            catch (PathTooLongException)
            {
                return (string)info.GetType()
                    .GetField("FullPath", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(info);
            }
        }
    }
}