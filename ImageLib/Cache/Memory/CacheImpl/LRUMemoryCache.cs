using Windows.Storage.Streams;

namespace ImageLib.Cache.Memory.CacheImpl
{
    /// <summary>
    /// LRU Memory Cache
    /// </summary>
    public class LRUMemoryCache : LRUCache<string, IRandomAccessStream>
    {
        private int _currentSize;

        public LRUMemoryCache(int capacity = 1024 * 1024 * 1024)
        {
            this._capacity = capacity;
        }

        protected override void CheckSize(string key, IRandomAccessStream value)
        {
            var size = (int)value.Size;
            _currentSize += size;
            while (_currentSize > _capacity && _lruList.Count > 0)
            {
                this.RemoveFirst();
            }
        }

        protected override void RemoveFirst()
        {
            var node = _lruList.First;
            var value = node.Value.Value;
            _currentSize -= (int)value.Size;
            base.RemoveFirst();
        }


    }
}
