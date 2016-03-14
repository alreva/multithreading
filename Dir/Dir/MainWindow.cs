using System;
using System.IO;
using System.Threading;
using System.Windows;
using Dir.Display;
using Dir.Read;

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
            Files = new FileSystemObjectCollection { new FileSystemNodeView("directory structure will go here...", FileSystemObjectSize.Undefined, FileSystemNodeViewType.Dir)};
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
           
            reader.DirectoryDiscovered += (_, d) => Dispatcher.BeginInvoke(new Action(() => Files.AddDir(d)));
            reader.FilesRead += (_, files) => Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (FileSystemNode file in files)
                {
                    Files.AddFile(file.Path, file.Size);
                }
            }));
            reader.DirectoryRead += (_, d) => Dispatcher.BeginInvoke(new Action(() => Files.SetSize(d.Path, d.Size)));
            reader.SecurityError += (_, path) => Dispatcher.BeginInvoke(new Action(() =>
            {
                Files.SetSize(path, 0);
                Files.SetError(path);
            }));

            string startPath = StartPath;
            var thread = new Thread(() => reader.Run(startPath));
            thread.Start();
        }
    }
}