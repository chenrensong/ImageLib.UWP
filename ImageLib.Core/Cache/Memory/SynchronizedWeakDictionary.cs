
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ImageLib.Cache.Memory
{
    public class SynchronizedWeakRefDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : class where TValue : class
    {
        private readonly WeakRefDictionary<TKey, TValue> _weakRefDictionary = new WeakRefDictionary<TKey, TValue>();
        private readonly object _lockObj = new object();

        private readonly IList<TKey> _keyList = new List<TKey>();

        public int Count { get; private set; }

        public bool IsReadOnly { get; private set; }

        public ICollection<TValue> Values { get; private set; }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _weakRefDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (_lockObj)
            {
                _weakRefDictionary.Add(item.Key, item.Value);
                _keyList.Add(item.Key);
                Count++;
            }
        }

        public void Clear()
        {
            lock (_lockObj)
            {
                foreach (var key in _keyList)
                {
                    _weakRefDictionary.Remove(key);
                }
                _keyList.Clear();
                Count = 0;
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (_lockObj)
            {
                TValue o;
                return _weakRefDictionary.TryGetValue(item.Key, out o);
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (_lockObj)
            {
                return _weakRefDictionary.ContainsKey(key);
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Add(TKey key, TValue value)
        {
            lock (_lockObj)
            {
                _weakRefDictionary.Add(key, value);
                _keyList.Add(key);
                Count++;
            }
        }



        public bool Remove(TKey key)
        {
            lock (_lockObj)
            {
                if (_keyList.Remove(key))
                {
                    Count--;
                    _weakRefDictionary.Remove(key);
                    return true;
                }

                return false;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_lockObj)
            {
                return _weakRefDictionary.TryGetValue(key, out value);
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (_lockObj)
                {
                    TValue value;
                    return _weakRefDictionary.TryGetValue(key, out value) ? value : null;
                }
            }

            set
            {
                lock (_lockObj)
                {
                    _weakRefDictionary.Remove(key);
                    _weakRefDictionary.Add(key, value);
                }
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                lock (_lockObj)
                {
                    return new List<TKey>(_keyList);
                }
            }
        }

    }
}
