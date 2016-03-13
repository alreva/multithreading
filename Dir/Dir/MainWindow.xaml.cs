using System;
using System.Collections.Generic;
using System.IO;
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
        public static readonly DependencyProperty FilesProperty = DependencyProperty.Register("Files", typeof (List<FileSystemNode>), typeof (MainWindow), new PropertyMetadata(default(List<FileSystemNode>)));

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

        public List<FileSystemNode> Files
        {
            get { return (List<FileSystemNode>) GetValue(FilesProperty); }
            set { SetValue(FilesProperty, value); }
        }

        public MainWindow()
        {
            InitializeComponent();

            StartPath = Environment.CurrentDirectory;
            Files = new List<FileSystemNode> {new FileSystemNode("sample.txt", new FileSystemObjectSize(10000))};
        }

        private void OnLoadStarted(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(StartPath))
            {
                StatusText = "Directory does not exist";
                return;
            }

            StatusText = StartPath;


        }
    }

    public class FileSystemNode
    {
        public FileSystemNode(string name, FileSystemObjectSize size)
        {
            Name = name;
            Size = size;
        }

        public string Name { get; set; }
        public FileSystemObjectSize Size { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Size})";
        }
    }

    public class FileSystemObjectSize
    {
        private readonly long _sizeInBytes;

        public FileSystemObjectSize(long sizeInBytes)
        {
            _sizeInBytes = sizeInBytes;
        }

        public override string ToString()
        {
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
    }
}