using System;
using System.IO;
using System.Windows;
using Dir.Display;
using Dir.Read;

namespace Dir
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty StatusTextProperty = DependencyProperty.Register(nameof(StatusText), typeof (string), typeof (MainWindow), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty StartPathProperty = DependencyProperty.Register(nameof(StartPath), typeof (string), typeof (MainWindow), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty FilesProperty = DependencyProperty.Register(nameof(Files), typeof (FileSystemObjectCollection), typeof (MainWindow), new PropertyMetadata(default(FileSystemObjectCollection)));

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

            StartNewSession();
            string startPath = StartPath;


            // There is not reason to introduce a new type here. This is nore for readability:
            // Window - for storing UI, DirProcessingflow - for processing.
            DirProcessingflow.Run(Dispatcher, Files, startPath);
        }


        private void StartNewSession()
        {
            StatusText = StartPath;
            Files.Clear();
        }
    }
}