using System;
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

        public FileDto[] Files { get; } = new FileDto[]
        {
            new FileDto("sample.txt", 100),
        };

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

        public MainWindow()
        {
            InitializeComponent();

            StartPath = Environment.CurrentDirectory;
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

    public class FileDto
    {
        public FileDto(string name, long size)
        {
            Name = name;
            Size = size;
        }

        public string Name { get; set; }
        public long Size { get; set; }
    }
}