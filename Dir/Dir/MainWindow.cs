using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Dir.Display;
using Dir.Mutithreading;
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
        public static readonly DependencyProperty OutputPathProperty = DependencyProperty.Register(nameof(OutputPath), typeof(string), typeof(MainWindow), new PropertyMetadata(default(string)));

        private DirProcessingflow _flow;

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

        public string OutputPath
        {
            get { return (string) GetValue(OutputPathProperty); }
            set { SetValue(OutputPathProperty, value); }
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
            string outputPath = OutputPath;


            // There is no reason to introduce a new type here. This is more for readability:
            // Window - for storing UI, DirProcessingflow - for processing.
            _flow = DirProcessingflow.Run(Dispatcher, Files, startPath, outputPath);
        }


        private void StartNewSession()
        {
            StatusText = StartPath;
            OutputPath = $"dir_{DateTime.Now.ToFileTime()}.xml";
            Files.Clear();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _flow?.Terminate();
        }

        private void OpenGeneratedFile(object sender, RoutedEventArgs e)
        {
            var filePath = OutputPath;
            if (!File.Exists(filePath))
            {
                return;
            }

            Process.Start(filePath);
        }
    }
}