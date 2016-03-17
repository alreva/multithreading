using System;
using System.Collections.Generic;

namespace Dir
{
    public class FlushingBuffer<T>
    {
        public int BufferSize { get; set; }
        private readonly Queue<T> _queue = new Queue<T>();

        public FlushingBuffer(int bufferSize = 10)
        {
            if (bufferSize < 1)
            {
                bufferSize = 10;
            }

            BufferSize = bufferSize;
        }

        public void Add(T item)
        {
            _queue.Enqueue(item);
            FlushIfBufferExceeded();
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                _queue.Enqueue(item);
            }

            FlushIfBufferExceeded();
        }

        public void AddRange(params T[] items)
        {
            AddRange((IEnumerable<T>)items);
        }

        public void FlushImmediately()
        {
            if (_queue.Count == 0)
            {
                return;
            }

            var items = new List<T>();
            while (_queue.Count > 0)
            {
                items.Add(_queue.Dequeue());
            }

            OnFlushed(items.ToArray());
        }

        public event EventHandler<T[]> Flushed;

        protected virtual void OnFlushed(T[] e)
        {
            Flushed?.Invoke(this, e);
        }

        private void FlushIfBufferExceeded()
        {
            if (BufferSize >= _queue.Count)
            {
                return;
            }

            FlushImmediately();
        }
    }
}
