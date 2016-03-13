using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;

namespace Dir
{
    public class DirectoryReader
    {
        public event EventHandler<string> DirectoryDiscovered;
        public event EventHandler<IEnumerable<FileSystemNode>> FilesRead;
        public event EventHandler<FileSystemNode> DirectoryRead;
        public event EventHandler<FileSystemNode> SecurityError;

        public void Run(string path)
        {
            LoadDirectoryInternal(path);
        }

        private long LoadDirectoryInternal(string path)
        {
            path = path.AppendDirectorySeparatorIfMissing();

            DirectoryInfo directory;
            try
            {
                directory = new DirectoryInfo(path);
            }
            catch (SecurityException)
            {
                OnSecurityError(new FileSystemNode(path, FileSystemObjectSize.Undefined));
                return 0;
            }

            string[] subDirectoryPaths;
            try
            {
                subDirectoryPaths = Directory.GetDirectories(path);
            }
            catch (UnauthorizedAccessException e)
            {
                OnSecurityError(new FileSystemNode(path, FileSystemObjectSize.Undefined));
                return 0;
            }

            foreach (string subDirectoryPath in subDirectoryPaths)
            {
                OnDirectoryDiscovered(subDirectoryPath);
            }

            FileInfo[] files = directory.GetFiles();

            OnFilesRead(files.Select(f => new FileSystemNode(f.GetFullPath(), f.Length)));

            long fullSize = files.Sum(f => f.Length);

            foreach (string subDirectoryPath in subDirectoryPaths)
            {
                long subDirectorySize = LoadDirectoryInternal(subDirectoryPath);
                fullSize += subDirectorySize;
            }

            OnDirectoryRead(new FileSystemNode(directory.GetFullPath(), fullSize));

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

        protected virtual void OnSecurityError(FileSystemNode e)
        {
            SecurityError?.Invoke(this, e);
        }
    }
}