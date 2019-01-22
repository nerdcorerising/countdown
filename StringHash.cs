

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace countdown
{
    /// <summary>
    /// The main reason I made this as a separate class instead of using HashSet<T> is that I wanted to 
    /// be able to check if a string was included without having to create a new string first.
    /// </summary>
    public class StringHash
    {
        // Constants used for the fnv-1a hash
        private const uint fnv32Offset = 2166136261u;
        private const uint fnv32Prime = 16777619u;

        // Hash an array of chars using fnv-1a. This function gives the same results 
        // as if the other hashing functions were called with identical contents.
        private static uint Hash(ReadOnlySpan<char> buffer)
        {
            uint hash = fnv32Offset;

            for (var i = 0; i < buffer.Length; i++)
            {
                hash = hash ^ buffer[i];
                hash *= fnv32Prime;
            }

            return hash;
        }

        // Same as Equal(string, string), but allows checking with a char * so it doesn't have to be
        // converted in to a string first, and can be called from an unsafe context.
        private static bool Equal(ReadOnlySpan<char> lhs, ReadOnlySpan<char> rhs)
        {
            if (lhs.Length != rhs.Length)
            {
                return false;
            }

            for (int i = 0; i < rhs.Length; ++i)
            {
                if (lhs[i] != rhs[i])
                {
                    return false;
                }
            }

            return true;
        }

        internal struct Node
        {
            public Node(uint hash, string value)
            {
                Hash = hash;
                Value = value;
                Initialized = true;
            }

            public bool Initialized { get; private set; }
            public uint Hash { get; private set; }
            public string Value { get; private set; }
        }

        // The number of buckets created on startup
        private const int InitialSize = 100;
        // How many collisions can occur before we grow
        private const double MaxLoadFactor = 0.33;

        private const double GrowFactor = 1.5;

        private int _count;

        private Node[] _data;

        private void Resize()
        {
            Node[] newData = new Node[(int)(_data.Length * GrowFactor)];

            int newCount = 0;
            foreach (Node n in _data)
            {
                if (n.Initialized)
                {
                    AddInternal(newData, n.Value, n.Hash, ref newCount);
                }
            }

            _data = newData;
            _count = newCount;
        }

        private bool AddInternal(Node[] data, ReadOnlySpan<char> item, uint hash, ref int count)
        {
            int pos = (int)(hash % data.Length);
            while (true)
            {
                if (pos >= data.Length)
                {
                    pos = 0;
                }

                if (!data[pos].Initialized)
                {
                    data[pos] = new Node(hash, new string(item));
                    count++;
                    return true;
                }

                if (data[pos].Hash == hash && Equal(data[pos].Value, item))
                {
                    return false;
                }

                ++pos;
            }
        }

        public StringHash(int size = InitialSize)
        {
            _data = new Node[size];
        }

        public bool Add(ReadOnlySpan<char> item)
        {
            if (((double)_count / (double)_data.Length) >= MaxLoadFactor)
            {
                Resize();
            }

            return AddInternal(_data, item, Hash(item), ref _count);
        }

        public void AddRange(StringHash hash)
        {
            for (int i = 0; i < hash._data.Length; ++i)
            {
                if (hash._data[i].Initialized)
                {
                    Add(hash._data[i].Value);
                }
            }
        }

        public bool Contains(ReadOnlySpan<char> item)
        {
            uint hash = Hash(item);
            int pos = (int)(hash % _data.Length);

            int i = pos;
            while (true)
            {
                if (i >= _data.Length)
                {
                    i = 0;
                }

                if (!_data[i].Initialized)
                {
                    return false;
                }

                if (_data[i].Hash == hash && Equal(_data[i].Value, item))
                {
                    return true;
                }

                ++i;
            }
        }

        public int Count()
        {
            return _count;
        }

        public IEnumerable<string> EnumerateItems()
        {
            foreach (Node n in _data)
            {
                if (n.Initialized)
                {
                    yield return n.Value;
                }
            }
        }
    }
}