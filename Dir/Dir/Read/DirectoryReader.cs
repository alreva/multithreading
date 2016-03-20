using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
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

            long totalSize = 0;

            foreach (string subDirectoryPath in subDirectoryPaths)
            {
                long subDirectorySize = LoadDirectoryInternal(subDirectoryPath);
                totalSize += subDirectorySize;
            }

            FileInfo[] files = directory.GetFiles();
            OnFilesRead(files.Select(file => FileSystemNodeBuilder.Create(file)));
            totalSize += files.Sum(f => f.Length);

            OnDirectoryRead(FileSystemNodeBuilder.Create(directory, totalSize));
            return totalSize;
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

        private static bool TryGetPermissions(FileInfo file, out string permissions)
        {
            permissions = null;

            try
            {
                AuthorizationRuleCollection rules = file.GetAccessControl().GetAccessRules(true, true, typeof (SecurityIdentifier));
                permissions = string.Join(", ", rules.OfType<FileSystemAccessRule>().Select(rule => $"{rule.AccessControlType} {rule.FileSystemRights}"));
                return !string.IsNullOrWhiteSpace(permissions);
            }
            catch (IOException)
            {
                return false;
            }
            catch (PrivilegeNotHeldException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        private string GetFileAttributes(string filePath)
        {
            return File.GetAttributes(filePath).ToString();
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