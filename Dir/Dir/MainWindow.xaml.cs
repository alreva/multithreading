using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Windows;

namespace Dir
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty StatusTextProperty = DependencyProperty.Register("StatusText", typeof (string), typeof (MainWindow), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty StartPathProperty = DependencyProperty.Register("StartPath", typeof (string), typeof (MainWindow), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty FilesProperty = DependencyProperty.Register("Files", typeof (FileSystemObjectCollection), typeof (MainWindow), new PropertyMetadata(default(FileSystemObjectCollection)));

        public string StartPath
        {
            get { return (string) GetValue(StartPathProperty); }
            set { SetValue(StartPathProperty, value); }
        }

        public string StatusText
        {
            get { return (string) GetValue(StatusTextProperty); }
            set { SetValue(StatusTextProperty, value); }
        }

        public FileSystemObjectCollection Files
        {
            get { return (FileSystemObjectCollection) GetValue(FilesProperty); }
            set { SetValue(FilesProperty, value); }
        }

        public MainWindow()
        {
            InitializeComponent();

            StartPath = Environment.CurrentDirectory;
            Files = new FileSystemObjectCollection { new FileSystemNode("sample.txt", new FileSystemObjectSize(10000))};
        }

        private void OnLoadStarted(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(StartPath))
            {
                StatusText = "Directory does not exist";
                return;
            }

            StatusText = StartPath;

            Files.Clear();

            var reader = new DirectoryReader();

            reader.DirectoryDiscovered += (_, d) => Files.AddDir(d);
            reader.FilesRead += (_, files) =>
            {
                foreach (FileSystemNode file in files)
                {
                    Files.AddFile(file.Path, file.Size);
                }
            };

            reader.Run(StartPath);
        }
    }

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
            OnDirectoryDiscovered(path);

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

            FileInfo[] files = directory.GetFiles();

            OnFilesRead(files.Select(f => new FileSystemNode(f.FullName, f.Length)));

            long fullSize = files.Sum(f => f.Length);

            string[] subDirectoruPaths = Directory.GetDirectories(path);
            foreach (string subDirectoryPath in subDirectoruPaths)
            {
                long subDirectorySize = LoadDirectoryInternal(subDirectoryPath);
                fullSize += subDirectorySize;
            }

            OnDirectoryRead(new FileSystemNode(directory.FullName, fullSize));

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

    public class FileSystemNode
    {
        public FileSystemNode(string path, FileSystemObjectSize size)
        {
            Path = path;
            Size = size;
        }

        public string Path { get; set; }
        public FileSystemObjectSize Size { get; set; }

        public override string ToString()
        {
            return $"{Path} ({Size})";
        }
    }

    public class FileSystemObjectCollection : ObservableCollection<FileSystemNode>
    {
        private readonly Dictionary<string, DirNode> _repo = new Dictionary<string, DirNode>();
        
        public void AddDir(string path)
        {
            if (_repo.ContainsKey(path))
            {
                return;
            }

            var nodeToAdd = new DirNode(Path.GetDirectoryName(path), FileSystemObjectSize.Undefined);
            _repo.Add(path, nodeToAdd);
            AddToHierarchy(path, nodeToAdd);
        }

        public void AddFile(string path, FileSystemObjectSize size)
        {
            var nodeToAdd = new FileSystemNode(Path.GetFileName(path), size);
            AddToHierarchy(path, nodeToAdd);
        }

        private void AddToHierarchy(string path, FileSystemNode nodeToAdd)
        {
            DirNode dir;
            if (TryGetParentFor(path, out dir))
            {
                dir.Children.Add(dir);
            }
            else
            {
                Add(nodeToAdd);
            }
        }

        private bool TryGetParentFor(string path, out DirNode dir)
        {
            string parentPath;
            if (!PathHelper.TryGetParentPath(path, out parentPath))
            {
                dir = null;
                return false;
            }

            return _repo.TryGetValue(parentPath, out dir);
        }

        public void UpdateSize(string path, long size)
        {
            DirNode dirNode;
            if (!_repo.TryGetValue(path, out dirNode))
            {
                return;
            }

            dirNode.Size = size;
        }
    }

    public class DirNode : FileSystemNode
    {
        public DirNode(
            string path,
            FileSystemObjectSize size,
            params FileSystemNode[] children)
            : base(path, size)
        {
            Children = new ObservableCollection<FileSystemNode>(children);
        }

        public ObservableCollection<FileSystemNode> Children { get; set; }
    }

    public class FileSystemObjectSize
    {
        private readonly long _sizeInBytes;

        public static readonly FileSystemObjectSize Undefined = new FileSystemObjectSize(0);

        public FileSystemObjectSize(long sizeInBytes)
        {
            _sizeInBytes = sizeInBytes;
        }

        public override string ToString()
        {
            if (!IsDefined)
            {
                return "";
            }

            if (_sizeInBytes > 1 << 30)
            {
                return $"{_sizeInBytes/(1 << 30)} GB";
            }

            if (_sizeInBytes > 1 << 20)
            {
                return $"{_sizeInBytes/(1 << 20)} MB";
            }

            if (_sizeInBytes > 1 << 10)
            {
                return $"{_sizeInBytes/(1 << 10)} KB";
            }

            return _sizeInBytes.ToString();
        }

        public bool IsDefined => _sizeInBytes > 0;

        public static implicit operator FileSystemObjectSize(long value)
        {
            return new FileSystemObjectSize(value);
        }
    }

    public static class PathHelper
    {
        public static bool TryGetParentPath(string path, out string parentPath)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            var tokens = path.Split(Path.DirectorySeparatorChar);

            if (tokens.Length == 1)
            {
                parentPath = null;
                return false;
            }

            parentPath = string.Join(Path.DirectorySeparatorChar.ToString(), tokens.Take(tokens.Length - 1));
            return true;
        }
    }
}