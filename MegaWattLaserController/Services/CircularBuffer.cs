// File: MegaWattLaserController/Services/CircularBuffer.cs
using System;
using System.Collections;
using System.Collections.Generic;

namespace LaserControllerApp.Services
{
    public class CircularBuffer<T> : IEnumerable<T>
    {
        private readonly T[] _buffer;
        private int _head;
        private int _count;
        private readonly int _capacity;

        public CircularBuffer(int capacity)
        {
            if (capacity <= 0) throw new ArgumentException("Capacity must be positive.", nameof(capacity));
            _capacity = capacity;
            _buffer = new T[capacity];
            _head = 0;
            _count = 0;
        }

        public int Count => _count;
        public int Capacity => _capacity;
        public bool IsFull => _count == _capacity;

        public void Add(T item)
        {
            _buffer[_head] = item;
            _head = (_head + 1) % _capacity;
            if (_count < _capacity) _count++;
        }

        public void Clear()
        {
            Array.Clear(_buffer, 0, _capacity);
            _head = 0;
            _count = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_count == 0) yield break;

            int start = _count < _capacity ? 0 : _head;
            for (int i = 0; i < _count; i++)
            {
                yield return _buffer[(start + i) % _capacity];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}