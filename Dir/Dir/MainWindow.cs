using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using Dir.Display;
using Dir.Mutithreading;
using Dir.Read;
using IWin32Window = System.Windows.Forms.IWin32Window;

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

        public bool IsOutputPathSelectedManually { get; set; }
        public bool IsOutputPathOverwriteConfirmed { get; set; }

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


        private bool StartNewSession()
        {
            if (IsOutputPathSelectedManually
                && File.Exists(OutputPath)
                && !IsOutputPathOverwriteConfirmed
                && System.Windows.MessageBox.Show("The output file will be deleted and you will loose the previous output. Are you sure you want to proceed?", "Output File Replacement Confirmation", MessageBoxButton.YesNo) != MessageBoxResult.OK)
            {
                return false;
            }

            StatusText = StartPath;
            if (!IsOutputPathSelectedManually)
            {
                OutputPath = $"dir_{DateTime.Now.ToFileTime()}.xml";
            }
            Files.Clear();
            IsOutputPathOverwriteConfirmed = false;
            return true;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _flow?.Terminate();
        }

        private void OpenGeneratedFile(object sender, RoutedEventArgs e)
        {
            string filePath = OutputPath;
            if (!File.Exists(filePath))
            {
                return;
            }

            Process.Start(filePath);
        }

        private void OnStartDirSelecting(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            DialogResult result = dlg.ShowDialog(GetIWin32Window(this));

            if (result != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            StartPath = dlg.SelectedPath;
        }

        private static IWin32Window GetIWin32Window(Visual visual)
        {
            var source = PresentationSource.FromVisual(visual) as HwndSource;
            IWin32Window win = new OldWindow(source.Handle);
            return win;
        }

        private class OldWindow : IWin32Window
        {
            private readonly IntPtr _handle;
            public OldWindow(IntPtr handle)
            {
                _handle = handle;
            }

            #region IWin32Window Members
            IntPtr IWin32Window.Handle
            {
                get { return _handle; }
            }
            #endregion
        }

        private void OnOutPathSelecting(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            DialogResult result = dlg.ShowDialog(GetIWin32Window(this));

            if (result != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            IsOutputPathSelectedManually = true;
            IsOutputPathOverwriteConfirmed = true;
            OutputPath = dlg.FileName;
        }
    }
}