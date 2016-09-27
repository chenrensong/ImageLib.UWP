using System;
using System.Collections.Generic;

namespace ImageLib.Cache.Memory
{
    /// <summary>
    /// LRU Cache
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class LRUCache<TKey, TValue> : MemoryCacheBase<TKey, TValue> where TKey : class where TValue : class
    {
        private readonly object _lockObj = new object();

        private Dictionary<TKey, LinkedListNode<LRUCacheItem<TKey, TValue>>> _cacheMap
            = new Dictionary<TKey, LinkedListNode<LRUCacheItem<TKey, TValue>>>();

        protected LinkedList<LRUCacheItem<TKey, TValue>> _lruList = new LinkedList<LRUCacheItem<TKey, TValue>>();

        /// <summary>
        /// Capacity
        /// </summary>
        protected int _capacity;

        public LRUCache(int capacity = 20)
        {
            this._capacity = capacity;
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue result;
                if (TryGetValue(key, out result))
                    return result;
                throw new KeyNotFoundException();
            }
        }

        public bool ContainsKey(TKey key)
        {
            TValue dummy;
            return TryGetValue(key, out dummy);
        }

        public override TValue Get(TKey key)
        {
            lock (_lockObj)
            {
                LinkedListNode<LRUCacheItem<TKey, TValue>> node;
                if (_cacheMap.TryGetValue(key, out node))
                {
                    TValue value = node.Value.Value;
                    _lruList.Remove(node);
                    _lruList.AddLast(node);
                    return value;
                }
                return default(TValue);
            }
        }

        public override void Put(TKey key, TValue value)
        {
            lock (_lockObj)
            {
                if (_cacheMap.ContainsKey(key))
                {
                    return;
                }
                this.CheckSize(key, value);
                LRUCacheItem<TKey, TValue> cacheItem = new LRUCacheItem<TKey, TValue>(key, value);
                LinkedListNode<LRUCacheItem<TKey, TValue>> node =
                    new LinkedListNode<LRUCacheItem<TKey, TValue>>(cacheItem);
                _lruList.AddLast(node);
                _cacheMap.Add(key, node);
            }
        }

        public override bool TryGetValue(TKey key, out TValue value)
        {
            lock (_lockObj)
            {
                LinkedListNode<LRUCacheItem<TKey, TValue>> node;
                if (_cacheMap.TryGetValue(key, out node))
                {
                    value = node.Value.Value;
                    _lruList.Remove(node);
                    _lruList.AddLast(node);
                    return true;
                }
                value = default(TValue);
                return false;
            }
        }

        public override void Clear()
        {
            _cacheMap.Clear();
            _lruList.Clear();
        }

        /// <summary>
        /// 检测目前内存大小
        /// </summary>
        protected virtual bool CheckSize(TKey key, TValue value)
        {
            if (_cacheMap.Count >= _capacity)
            {
                RemoveFirst();
            }
            return true;
        }

        public void Remove(TKey key)
        {
            LinkedListNode<LRUCacheItem<TKey, TValue>> node;
            if (_cacheMap.TryGetValue(key, out node))
            {
                _lruList.Remove(node);
            }
        }

        protected virtual void RemoveNode(LinkedListNode<LRUCacheItem<TKey, TValue>> node)
        {
            _lruList.Remove(node);
            _cacheMap.Remove(node.Value.Key);
        }

        protected void RemoveFirst()
        {
            // Remove from LRUPriority
            LinkedListNode<LRUCacheItem<TKey, TValue>> node = _lruList.First;
            this.RemoveNode(node);

            //_lruList.RemoveFirst();
            //// Remove from cache
            //_cacheMap.Remove(node.Value.Key);
        }


        /// <summary>
        /// LRU Cache Item
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        protected class LRUCacheItem<K, V>
        {
            public LRUCacheItem(K k, V v)
            {
                Key = k;
                Value = v;
            }
            public K Key
            {
                get;
                private set;
            }
            public V Value
            {
                get;
                private set;
            }
        }
    }




}
