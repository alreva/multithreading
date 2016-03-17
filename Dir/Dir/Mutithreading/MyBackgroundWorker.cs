using System;
using System.Collections.Generic;
using System.Threading;

namespace Dir.Mutithreading
{
    public class MyBackgroundWorker : IDisposable
    {
        private readonly int _queueSize;
        private ManualResetEvent _enqueueBlocker = new ManualResetEvent(true);
        private ManualResetEvent _workBlocker = new ManualResetEvent(false);
        private Thread _workingThread;

        private readonly Queue<Action> _workQueue = new Queue<Action>();

        private volatile bool _shouldStop;
        private volatile bool _hasStarted;
        private bool _isDisposed;

        public MyBackgroundWorker(int queueSize = 10)
        {
            _queueSize = queueSize > 0 ? queueSize : 10;
        }

        public void Enqueue(Action work)
        {
            ValidateNotDisposed();

            _enqueueBlocker.WaitOne(TimeSpan.FromMinutes(1));
            _workQueue.Enqueue(work);
            FlushIfFull();
        }

        public void Start()
        {
            ValidateNotDisposed();

            if (_hasStarted)
            {
                return;
            }

            _workingThread = new Thread(() =>
            {
                while (!_shouldStop)
                {
                    _workBlocker.WaitOne(TimeSpan.FromMinutes(1));

                    var actions = new List<Action>();

                    while (_workQueue.Count > 0)
                    {
                        actions.Add(_workQueue.Dequeue());
                    }

                    _workBlocker.Reset();
                    _enqueueBlocker.Set();

                    actions.ForEach(a => a());
                }
            });

            _workingThread.Start();
            _hasStarted = true;
        }

        public void Close()
        {
            Stop();
            Dispose();
        }

        public void Stop()
        {
            ValidateNotDisposed();

            if (!_hasStarted)
            {
                return;
            }

            _shouldStop = true;
            _workBlocker.Set();
            _workingThread.Join(TimeSpan.FromMinutes(1));
            _hasStarted = false;
        }

        private void FlushIfFull()
        {
            if (_workQueue.Count > _queueSize)
            {
                FlushImmediately();
            }
        }

        private void FlushImmediately()
        {
            _enqueueBlocker.Reset();
            _workBlocker.Set();
            _enqueueBlocker.WaitOne();
        }

        private void ValidateNotDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MyBackgroundWorker()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool calledExplicitly)
        {
            if (_isDisposed)
            {
                return;
            }

            if (calledExplicitly)
            {
                _enqueueBlocker.Close();
                _workBlocker.Close();
            }

            _enqueueBlocker = null;
            _workBlocker = null;

            _isDisposed = true;
        }
    }
}
