﻿using System;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using System.Xml;
using Dir.Display;
using Dir.Read;

namespace Dir.Mutithreading
{
    public class DirProcessingflow
    {
        private readonly string _startPath;
        private readonly string _outputPath;
        private readonly DirectoryReader _reader;
        private readonly FileSystemObjectCollection _files;
        private readonly Dispatcher _dispatcher;

        public static DirProcessingflow Run(Dispatcher dispatcher, FileSystemObjectCollection files, string startPath, string outputPath)
        {
            var flow = new DirProcessingflow(startPath, outputPath, dispatcher, files, new DirectoryReader());
            flow.SetUpUiUpdates();
            flow.SetUpXmlWrites();
            flow.Start();

            return flow;
        }

        public void Terminate()
        {
            try
            {
                _reader.TerminateImmediately();
            }
            catch (ThreadAbortException)
            {
                // do nothing here, this is expected
            }
        }

        private void Start()
        {
            _reader.Run(_startPath);
        }

        private DirProcessingflow(
            string startPath,
            string outputPath,
            Dispatcher dispatcher,
            FileSystemObjectCollection files,
            DirectoryReader reader)
        {
            _startPath = startPath;
            _outputPath = outputPath;
            _reader = reader;
            _dispatcher = dispatcher;
            _files = files;
        }

        private void SetUpUiUpdates()
        {
            // Need to have a low queue length since the smaller the UI change - the less time the UI thread is blocked.
            // In fact, I would not do multithreaded UI output here; this is due to the constraint in the requirements to have a UI output thread.
            var worker = new MyBackgroundWorker(1);
            worker.Start();


            _reader.DirectoryDiscovered += (_, path) =>
            {
                EnqueueDispatcherWork(() => _files.AddDir(path), worker);
            };

            _reader.FilesRead += (_, files) =>
            {
                foreach (FileSystemNode file in files)
                {
                    EnqueueDispatcherWork(() => _files.AddFile(file.Path, file.Size), worker);
                }
            };

            _reader.DirectoryRead += (_, dir) =>
            {
                EnqueueDispatcherWork(() => _files.SetSize(dir.Path, dir.Size), worker);
            };

            _reader.SecurityError += (_, path) =>
            {
                EnqueueDispatcherWork(() => _files.SetSize(path, 0), worker);
                EnqueueDispatcherWork(() => _files.SetError(path), worker);
            };

            _reader.Complete += (sender, args) =>
            {
                worker.Close();
            };

            _reader.Terminating += (sender, args) =>
            {
                worker.Dispose();
            };
        }

        private void EnqueueDispatcherWork(Action work, MyBackgroundWorker worker)
        {
            worker.Enqueue(() => _dispatcher.BeginInvoke(work));
        }

        private void SetUpXmlWrites()
        {
            // It is more common to write to a file in a batch rather that record-by-record.
            var worker = new MyBackgroundWorker(1000);

            worker.Start();

            XmlWriter xmlW = XmlWriter.Create(
                File.OpenWrite(_outputPath),
                new XmlWriterSettings { CloseOutput = true, Indent = true });

            worker.Enqueue(() =>
            {
                xmlW.WriteStartDocument();
                xmlW.WriteStartElement("analysis");
                xmlW.WriteAttributeString("path", _startPath);
            });

            _reader.DirectoryDiscovered += (_, dirPath) =>
            {
                worker.Enqueue(() => xmlW.WriteStartElement("dir"));
                worker.Enqueue(() => xmlW.WriteAttributeString("name", dirPath.GetShortDirectoryName()));
            };

            _reader.FilesRead += (_, files) =>
            {
                foreach (FileSystemNode file in files)
                {
                    worker.Enqueue(() => xmlW.WriteStartElement("file"));
                    worker.Enqueue(() => xmlW.WriteAttributeString("name", Path.GetFileName(file.Path)));
                    if (file.Size.IsDefined)
                    {
                        worker.Enqueue(() => xmlW.WriteAttributeString("size", file.Size.ToString()));
                    }

                    foreach (NameValue nameValue in file.Properties)
                    {
                        worker.Enqueue(() => xmlW.WriteAttributeString(nameValue.Name, nameValue.Value));
                    }

                    worker.Enqueue(() => xmlW.WriteEndElement());
                }
            };

            _reader.DirectoryRead += (_, dir) =>
            {
                if (dir.Size.IsDefined)
                {
                    worker.Enqueue(() => xmlW.WriteElementString("size", dir.Size.ToString()));
                    foreach (NameValue nameValue in dir.Properties)
                    {
                        worker.Enqueue(() => xmlW.WriteElementString(nameValue.Name, nameValue.Value));
                    }
                }
                worker.Enqueue(() => xmlW.WriteEndElement());
            };

            _reader.SecurityError += (_, dirPath) =>
            {
                worker.Enqueue(() => xmlW.WriteStartElement("securityError"));
                worker.Enqueue(() => xmlW.WriteEndElement());
                worker.Enqueue(() => xmlW.WriteEndElement());
            };

            _reader.Complete += (_, __) =>
            {
                worker.Enqueue(() => xmlW.WriteEndElement());
                worker.Enqueue(() => xmlW.WriteEndDocument());
                worker.Enqueue(() => xmlW.Close());
                worker.Enqueue(() => xmlW = null);

                worker.Close();
            };

            _reader.Terminating += (sender, args) =>
            {
                worker.Enqueue(() => xmlW.Close());
                worker.Enqueue(() => xmlW = null);
                worker.Dispose();
            };
        }
    }
}
