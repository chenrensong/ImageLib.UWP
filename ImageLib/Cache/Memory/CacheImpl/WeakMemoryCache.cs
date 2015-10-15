namespace ImageLib.Cache.Memory.CacheImpl
{
    public class WeakMemoryCache<TKey, TValue> : MemoryCacheBase<TKey, TValue> where TKey : class where TValue : class
    {
        private readonly SynchronizedWeakRefDictionary<TKey, TValue> _synchronizedWeakDictionary = new SynchronizedWeakRefDictionary<TKey, TValue>(); 

        public override TValue Get(TKey key)
        {
            return _synchronizedWeakDictionary[key];
        }

        public override void Put(TKey key, TValue value)
        {
            _synchronizedWeakDictionary[key] = value;
        }

        public override bool TryGetValue(TKey key, out TValue value)
        {
            return _synchronizedWeakDictionary.TryGetValue(key, out value);
        }

        public override void Clear()
        {
            _synchronizedWeakDictionary.Clear();
        }
    }
}
