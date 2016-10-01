using ImageLib.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace ImageLib.Cache.Storage.CacheImpl
{
    public class LimitedStorageCache : StorageCacheBase
    {
        /// <summary>
        /// Dictionary contains pairs of filePath and last access time in unix timestamp * 1000 (DateTime.Millisecond)
        /// </summary>
        private readonly IDictionary<string, long> _lastAccessTimeDictionary = new SynchronizedDictionary<string, long>();

        private readonly object _lockObject = new object();

        private long _currentCacheSizeInBytes = -1;

        protected long CurrentCacheSizeInBytes
        {
            get
            {
                lock (_lockObject)
                {
                    return _currentCacheSizeInBytes;
                }
            }

            set
            {
                lock (_lockObject)
                {
                    _currentCacheSizeInBytes = value;
                }
            }
        }

        private readonly long _cacheLimitInBytes;

        /// <summary>
        /// Creates new LimitedStorageCache instance
        /// </summary>
        /// <param name="isf">StorageFolder instance to work with file system</param>
        /// <param name="cacheDirectory">Directory to store cache, starting with two slashes "\\"</param>
        /// <param name="cacheFileNameGenerator">ICacheFileNameGenerator instance to generate cache filenames</param>
        /// <param name="cacheLimitInBytes">Limit of total cache size in bytes, for example 10 mb == 10 * 1024 * 1024</param>
        /// <param name="cacheMaxLifetimeInMillis">Cache max lifetime in millis, for example two weeks = 2 * 7 * 24 * 60 * 60 * 1000; default value == one week; pass value &lt;= 0 to disable max cache lifetime</param>
        public LimitedStorageCache(StorageFolder isf, string cacheDirectory,
            ICacheGenerator cacheFileNameGenerator, long cacheLimitInBytes, long cacheMaxLifetimeInMillis = DefaultCacheMaxLifetimeInMillis)
            : base(isf, cacheDirectory, cacheFileNameGenerator, cacheMaxLifetimeInMillis)
        {
            _cacheLimitInBytes = cacheLimitInBytes;
            BeginCountCurrentCacheSize();
        }

        public async override Task<bool> SaveAsync(string cacheKey, IRandomAccessStream cacheStream)
        {
            var fullFileName = Path.Combine(CacheDirectory, CacheFileNameGenerator.GenerateCacheName(cacheKey));
            var cacheSizeInBytes = cacheStream.AsStreamForRead().Length;

            while (CurrentCacheSizeInBytes + cacheSizeInBytes > _cacheLimitInBytes)
            {
                if (!await RemoveOldestCacheFile())
                {
                    break; // All cache deleted
                }
            }

            var wasFileSaved = await base.InternalSaveAsync(fullFileName, cacheStream);

            if (wasFileSaved)
            {
                _lastAccessTimeDictionary[Path.Combine(CacheDirectory, fullFileName)] = DateTimeHelper.CurrentTimeMillis();
                CurrentCacheSizeInBytes += cacheSizeInBytes; // Updating current cache size
            }

            return wasFileSaved;
        }

        private void BeginCountCurrentCacheSize()
        {
            Task.Factory.StartNew(async () =>
            {
                IReadOnlyList<StorageFile> cacheFiles;
                try
                {
                    var storageFolder = await SF.GetFolderAsync(CacheDirectory);
                    cacheFiles = await storageFolder.GetFilesAsync();
                }
                catch (Exception ex)
                {
                    return;
                }

                long cacheSizeInBytes = 0;

                foreach (var cacheFile in cacheFiles)
                {
                    var properties = await cacheFile.GetBasicPropertiesAsync();
                    var fullCacheFilePath = cacheFile.Name;
                    try
                    {
                        cacheSizeInBytes += (long)properties.Size;
                        _lastAccessTimeDictionary.Add(fullCacheFilePath, properties.DateModified.DateTime.Milliseconds());
                    }
                    catch
                    {
                        ImageLog.Log("[error] can not get cache's file size: " + fullCacheFilePath);
                    }
                }
                CurrentCacheSizeInBytes += cacheSizeInBytes; // Updating current cache size
            });
        }

        /// <summary>
        /// Removing oldest cache file (file, which last access time is smaller)
        /// </summary>
        private async Task<bool> RemoveOldestCacheFile()
        {
            if (_lastAccessTimeDictionary.Count == 0) return false;

            var oldestCacheFilePath = _lastAccessTimeDictionary.Aggregate((pair1, pair2) => (pair1.Value < pair2.Value) ? pair1 : pair2).Key;

            if (string.IsNullOrEmpty(oldestCacheFilePath)) return false;

            oldestCacheFilePath = Path.Combine(CacheDirectory, oldestCacheFilePath);

            try
            {
                long fileSizeInBytes;
                var storageFile = await SF.GetFileAsync(oldestCacheFilePath);
                var properties = await storageFile.GetBasicPropertiesAsync();
                fileSizeInBytes = (long)properties.Size;

                try
                {
                    await storageFile.DeleteAsync();
                    _lastAccessTimeDictionary.Remove(oldestCacheFilePath);
                    CurrentCacheSizeInBytes -= fileSizeInBytes; // Updating current cache size
                    ImageLog.Log("[delete] cache file " + oldestCacheFilePath);
                    return true;
                }
                catch
                {
                    ImageLog.Log("[error] can not delete oldest cache file: " + oldestCacheFilePath);
                }
            }
            catch (Exception ex)
            {
                ImageLog.Log("[error] can not get olders cache's file size: " + oldestCacheFilePath);
            }

            return false;
        }
    }
}
