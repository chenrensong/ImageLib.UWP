
namespace ImageLib.Cache.Memory
{
    public abstract class MemoryCacheBase<TKey, TValue> where TKey : class where TValue : class 
    {
        public abstract TValue Get(TKey key);

        public abstract void Put(TKey key, TValue value);

        public abstract bool TryGetValue(TKey key, out TValue value);

        public abstract void Clear();
    }
}
