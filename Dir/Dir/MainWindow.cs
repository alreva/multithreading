using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Xml;
using Dir.Display;
using Dir.Read;

namespace Dir
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ManualResetEvent _xmlEvent;
        private XmlWriter _xmlW;
        private volatile bool _xmlStop = false;

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

            var reader = new DirectoryReader();

            SetUpUiUpdates(reader);
            SetUpXmlWrites(reader, startPath);

            Run(reader, startPath);
        }

        private void SetUpXmlWrites(DirectoryReader reader, string startPath)
        {
            var wh = new ManualResetEvent(false);
            var wh2 = new ManualResetEvent(false);
            var q = new Queue<Action>();
            var thread = new Thread(() =>
            {
                while (!_xmlStop)
                {
                    wh.WaitOne(TimeSpan.FromMinutes(1));

                    var actions = new List<Action>();

                    while (q.Count > 0)
                    {
                        actions.Add(q.Dequeue());
                    }

                    wh.Reset();
                    wh2.Set();

                    actions.ForEach(a => a());
                }
            });

            thread.Start();

            _xmlEvent = new ManualResetEvent(false);
            _xmlW = XmlWriter.Create(
                File.OpenWrite($"dir_{DateTime.Now.ToFileTime()}.xml"),
                new XmlWriterSettings {CloseOutput = true, Indent = true});
            q.Enqueue(() => _xmlW.WriteStartDocument());
            q.Enqueue(() => _xmlW.WriteStartElement("analysis"));
            q.Enqueue(() => _xmlW.WriteAttributeString("path", startPath));


            reader.DirectoryDiscovered += (_, dirPath) =>
            {
                q.Enqueue(() => _xmlW.WriteStartElement("dir"));
                q.Enqueue(() => _xmlW.WriteAttributeString("name", dirPath.GetShortDirectoryName()));
            };

            reader.FilesRead += (_, files) =>
            {
                foreach (FileSystemNode file in files)
                {
                    q.Enqueue(() => _xmlW.WriteStartElement("file"));
                    q.Enqueue(() => _xmlW.WriteAttributeString("name", Path.GetFileName(file.Path)));
                    if (file.Size.IsDefined)
                    {
                        q.Enqueue(() => _xmlW.WriteAttributeString("size", file.Size.ToString()));
                    }
                    q.Enqueue(() => _xmlW.WriteEndElement());
                }
                PushXmlMessagesIfFull(q, wh, wh2);
            };

            reader.DirectoryRead += (_, dir) =>
            {
                if (dir.Size.IsDefined)
                {
                    q.Enqueue(() => _xmlW.WriteElementString("size", dir.Size.ToString()));
                }
                q.Enqueue(() => _xmlW.WriteEndElement());
                PushXmlMessagesIfFull(q, wh, wh2);
            };

            reader.SecurityError += (_, dirPath) =>
            {
                q.Enqueue(() => _xmlW.WriteStartElement("securityError"));
                q.Enqueue(() => _xmlW.WriteEndElement());
                q.Enqueue(() => _xmlW.WriteEndElement());
            };

            reader.Complete += (_, __) =>
            {
                q.Enqueue(() => _xmlW.WriteEndElement());
                q.Enqueue(() => _xmlW.WriteEndDocument());
                q.Enqueue(() => _xmlW.Dispose());
                q.Enqueue(() => _xmlEvent.Dispose());
                q.Enqueue(() => _xmlW = null);
                q.Enqueue(() => _xmlEvent = null);
                PushXmlMessages(q, wh, wh2);
                _xmlStop = true;
                wh.Set();
                thread.Join();
                wh.Dispose();
            };
        }

        private static void PushXmlMessagesIfFull(Queue<Action> queue, ManualResetEvent wh, ManualResetEvent wh2)
        {
            if (queue.Count > 1000)
            {
                PushXmlMessages(queue, wh, wh2);
            }
        }

        private static void PushXmlMessages(Queue<Action> queue, ManualResetEvent wh, ManualResetEvent wh2)
        {
            wh.Set();
            wh2.WaitOne();
            wh2.Reset();
        }

        private void SetUpUiUpdates(DirectoryReader reader)
        {
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


            var q = new FlushingBuffer<FileSystemNode>(100);
            q.Flushed += (_, files) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    StatusText = Files.First().Path;
                }));
            };

            reader.FilesRead += (_, files) => q.AddRange(files);
            reader.Complete += (_, __) => q.FlushImmediately();
        }

        private void Run(DirectoryReader reader, string startPath)
        {
            var thread = new Thread(() => reader.Run(startPath));
            thread.Start();
        }

        private void StartNewSession()
        {
            StatusText = StartPath;

            Files.Clear();
        }
    }
}