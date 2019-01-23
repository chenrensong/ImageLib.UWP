
using System.Collections;
using System.Collections.Generic;

namespace ImageLib.Cache.Storage
{
    /// <summary>
    /// Thread-safe IDictionary implementation
    /// It is not high-performance (simple locking implementation)
    /// </summary>
    public class SynchronizedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>(); 
        private readonly object _lockObj = new object();

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (_lockObj)
            {
                return _dictionary.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (_lockObj)
            {
                _dictionary.Add(item);
            }
        }

        public void Clear()
        {
            lock (_lockObj)
            {
                _dictionary.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (_lockObj)
            {
                return _dictionary.Contains(item);
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (_lockObj)
            {
                _dictionary.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (_lockObj)
            {
                return _dictionary.Remove(item);
            }
        }

        public int Count
        {
            get
            {
                lock (_lockObj)
                {
                    return _dictionary.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                lock (_lockObj)
                {
                    return _dictionary.IsReadOnly;
                }
            }
        }
        
        public void Add(TKey key, TValue value)
        {
            lock (_lockObj)
            {
                _dictionary.Add(key, value);
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (_lockObj)
            {
                return _dictionary.ContainsKey(key);
            }
        }

        public bool Remove(TKey key)
        {
            lock (_lockObj)
            {
                return _dictionary.Remove(key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_lockObj)
            {
                return _dictionary.TryGetValue(key, out value);
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (_lockObj)
                {
                    return _dictionary[key];
                }
            }

            set
            {
                lock (_lockObj)
                {
                    _dictionary[key] = value;
                }
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                lock (_lockObj)
                {
                    return _dictionary.Keys;
                }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                lock (_lockObj)
                {
                    return _dictionary.Values;
                }
            }
        }
    }
}
