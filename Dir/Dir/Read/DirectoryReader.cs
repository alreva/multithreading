using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading;

namespace Dir.Read
{
    public class DirectoryReader
    {
        private volatile bool _shouldTerminate = false;

        public event EventHandler<string> DirectoryDiscovered;
        public event EventHandler<IEnumerable<FileSystemNode>> FilesRead;
        public event EventHandler<FileSystemNode> DirectoryRead;
        public event EventHandler<string> SecurityError;
        public event EventHandler Complete;
        public event EventHandler Terminating;

        public void Run(string path)
        {
            new Thread(() =>
            {
                try
                {
                    LoadDirectoryInternal(path);
                }
                finally
                {
                    if (!_shouldTerminate)
                    {
                        OnComplete();
                    }
                }
            }).Start();
        }

        public void TerminateImmediately()
        {
            _shouldTerminate = true;
        }

        private long LoadDirectoryInternal(string path)
        {
            if (_shouldTerminate)
            {
                OnTerminating();
                Thread.CurrentThread.Abort();
                return 0;
            }

            if (path.Last() == '\\')
            {
                path = path.TrimEnd('\\');
            }

            OnDirectoryDiscovered(path);

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

            long fullSize = 0;

            foreach (string subDirectoryPath in subDirectoryPaths)
            {
                long subDirectorySize = LoadDirectoryInternal(subDirectoryPath);
                fullSize += subDirectorySize;
            }

            FileInfo[] files = directory.GetFiles();
            OnFilesRead(files.Select(file => new FileSystemNode(GetFullPath(file), file.Length)));
            fullSize += files.Sum(f => f.Length);

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

        protected virtual void OnComplete()
        {
            Complete?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnTerminating()
        {
            Terminating?.Invoke(this, EventArgs.Empty);
        }
    }
}