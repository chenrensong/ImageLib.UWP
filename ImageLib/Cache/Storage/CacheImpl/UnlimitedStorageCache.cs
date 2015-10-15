
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace ImageLib.Cache.Storage.CacheImpl
{
    /// <summary>
    /// Simpliest implemetation of StorageCacheBase
    /// Unlimited storage cache, it will never delete old cache files
    /// </summary>
    public class UnlimitedStorageCache : StorageCacheBase
    {
        /// <summary>
        /// Creates instance 
        /// </summary>
        /// <param name="isf">StorageFolder instance to work with file system</param>
        /// <param name="cacheDirectory">Directory to store cache, starting with two slashes "\\"</param>
        /// <param name="cacheFileNameGenerator">ICacheFileNameGenerator instance to generate cache filenames</param>
        /// <param name="cacheMaxLifetimeInMillis">Cache max lifetime in millis, for example two weeks = 2 * 7 * 24 * 60 * 60 * 1000; default value == 0; pass value &lt;= 0 to disable max cache lifetime</param>
        public UnlimitedStorageCache(StorageFolder isf, string cacheDirectory, ICacheGenerator cacheFileNameGenerator, long cacheMaxLifetimeInMillis = 0)
            : base(isf, cacheDirectory, cacheFileNameGenerator, cacheMaxLifetimeInMillis)
        {
           
        }

        /// <summary>
        /// Just calls StorageCacheBase.InternalSaveAsync() without any other operation
        /// </summary>
        /// <param name="cacheKey">will be used by CacheFileNameGenerator</param>
        /// <param name="cacheStream">will be written to the cache file</param>
        /// <returns>true if cache was saved, false otherwise</returns>
        public override Task<bool> SaveAsync(string cacheKey, IRandomAccessStream cacheStream)
        {
            return InternalSaveAsync(CacheFileNameGenerator.GenerateCacheName(cacheKey), cacheStream);
        }
    }   
}
