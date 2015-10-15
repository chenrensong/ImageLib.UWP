
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace ImageLib.Cache.Storage
{
    public abstract class StorageCacheBase
    {
        /// <summary>
        /// Default value of cache max lifetime in milliseconds
        /// Equals to one week 604800000 milliseconds == 7 * 24 * 60 * 60 * 1000
        /// </summary>
        protected const long DefaultCacheMaxLifetimeInMillis = 7 * 24 * 60 * 60 * 1000; // == 604800000;

        /// <summary>
        /// StorageFolder instance to work with app's ISF
        /// </summary>
        protected readonly StorageFolder ISF;

        /// <summary>
        /// Base cache directory where all cache will be saved
        /// </summary>
        protected virtual string CacheDirectory { get; set; }

        /// <summary>
        /// Generates file name from the cache key
        /// </summary>
        protected virtual ICacheGenerator CacheFileNameGenerator { get; set; }

        /// <summary>
        /// Maximum age of cache in milliseconds
        /// <= 0 — always alive
        /// </summary>
        protected virtual long CacheMaxLifetimeInMillis { get; set; }

        protected StorageCacheBase(StorageFolder isf, string cacheDirectory, ICacheGenerator cacheFileNameGenerator, long cacheMaxLifetimeInMillis = DefaultCacheMaxLifetimeInMillis)
        {
            if (isf == null)
            {
                throw new ArgumentNullException("isf");
            }

            if (string.IsNullOrEmpty(cacheDirectory))
            {
                throw new ArgumentException("cacheDirectory name could not be null or empty");
            }

            //if (!cacheDirectory.StartsWith("\\"))
            //{
            //    throw new ArgumentException("cacheDirectory name should starts with double slashes: \\");
            //}

            if (cacheFileNameGenerator == null)
            {
                throw new ArgumentNullException("cacheFileNameGenerator");
            }

            ISF = isf;
            CacheDirectory = cacheDirectory;
            CacheFileNameGenerator = cacheFileNameGenerator;
            CacheMaxLifetimeInMillis = cacheMaxLifetimeInMillis;

            // Creating cache directory if it not exists
            ISF.CreateFolderAsync(CacheDirectory).AsTask();
        }

        /// <summary>
        /// You should implement this method. Usefull to handle cache saving as you want
        /// Base implementation is InternalSaveAsync(), you can call it in your implementation
        /// </summary>
        /// <param name="cacheKey">will be used by CacheFileNameGenerator</param>
        /// <param name="cacheStream">will be written to the cache file</param>
        /// <returns>true if cache was saved, false otherwise</returns>
        public abstract Task<bool> SaveAsync(string cacheKey, IRandomAccessStream cacheStream);


        /// <summary>
        /// Saves the file with fullFilePath, uses FileMode.Create, so file create time will be rewrited if needed
        /// If exception has occurred while writing the file, it will delete it
        /// </summary>
        /// <param name="fullFilePath">example: "\\image_cache\\213898adj0jd0asd</param>
        /// <param name="cacheStream">stream to write to the file</param>
        /// <returns>true if file was successfully written, false otherwise</returns>
        protected async virtual Task<bool> InternalSaveAsync(string fullFilePath, IRandomAccessStream cacheStream)
        {
            var storageFile = await ISF.CreateFileAsync(fullFilePath, CreationCollisionOption.ReplaceExisting);

            using (IRandomAccessStream outputStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                try
                {
                    await RandomAccessStream.CopyAsync(
                        cacheStream.GetInputStreamAt(0L),
                        outputStream.GetOutputStreamAt(0L));
                }
                catch
                {
                    try
                    {
                        // If file was not saved normally, we should delete it
                        await storageFile.DeleteAsync();
                    }
                    catch
                    {
                        ImageLoader.Log("[error] can not delete unsaved file: " + fullFilePath);
                    }
                }
            }
            ImageLoader.Log("[error] can not save cache to the: " + fullFilePath);
            return false;
        }

        /// <summary>
        /// Async gets file stream by the cacheKey (cacheKey will be converted using CacheFileNameGenerator)
        /// </summary>
        /// <param name="cacheKey">key will be used by CacheFileNameGenerator to get cache's file name</param>
        /// <returns>Stream of that file or null, if it does not exists</returns>
        public async virtual Task<IRandomAccessStream> LoadCacheStreamAsync(string cacheKey)
        {
            var fullFilePath = GetFullFilePath(CacheFileNameGenerator.GenerateCacheName(cacheKey));

            try
            {
                var cacheFileMemoryStream = new InMemoryRandomAccessStream();
                var storageFile = await ISF.GetFileAsync(fullFilePath);
             
                using (var cacheFileStream = await storageFile.OpenAsync(FileAccessMode.Read))
                {
                    await RandomAccessStream.CopyAsync(
                        cacheFileStream.GetInputStreamAt(0L),
                        cacheFileMemoryStream.GetOutputStreamAt(0L));
                    return cacheFileMemoryStream;
                }
            }
            catch (Exception ex)
            {
                ImageLoader.Log("[error] can not load file stream from: " + fullFilePath);
                return null;
            }
        }

        /// <summary>
        /// Gets full file path, combining it with CacheDirectory
        /// </summary>
        /// <param name="fileName">name of the file</param>
        /// <returns>full path to the file</returns>
        protected virtual string GetFullFilePath(string fileName)
        {
            return Path.Combine(CacheDirectory, fileName);
        }

        /// <summary>
        /// Checks file existence
        /// </summary>
        /// <param name="cacheKey">Will be used by CacheFileNameGenerator</param>
        /// <returns>true if file with cache exists, false otherwise</returns>
        public virtual async Task<bool> IsCacheExists(string cacheKey)
        {
            var fullFilePath = GetFullFilePath(CacheFileNameGenerator.GenerateCacheName(cacheKey));

            try
            {
                await ISF.GetFileAsync(fullFilePath);
                return true;
            }
            catch
            {
                ImageLoader.Log("[error] can not check cache existence, file: " + fullFilePath);
                return false;
            }
        }

        /// <summary>
        /// Checks is cache existst and its last write time &lt;= CacheMaxLifetimeInMillis
        /// </summary>
        /// <param name="cacheKey">Will be used by CacheFileNameGenerator</param>
        /// <returns>true if cache exists and alive, false otherwise</returns>
        public virtual async Task<bool> IsCacheExistsAndAlive(string cacheKey)
        {
            var fullFilePath = GetFullFilePath(CacheFileNameGenerator.GenerateCacheName(cacheKey));

            try
            {
                var storageFile = await ISF.GetFileAsync(fullFilePath);
                return CacheMaxLifetimeInMillis <= 0 ? true :
                    ((DateTime.Now - storageFile.DateCreated.DateTime).TotalMilliseconds < CacheMaxLifetimeInMillis);
            }
            catch
            {
                ImageLoader.Log("[error] can not check is cache exists and alive, file: " + fullFilePath);
            }

            return false;
        }

        /// <summary>
        /// Deletes all cache from CacheDirectory
        /// </summary>
        public virtual async Task Clear()
        {
            await DeleteDirContent(CacheDirectory);
        }

        /// <summary>
        /// Recursive method to delete all content of needed directory
        /// </summary>
        /// <param name="absoluteDirPath">Path of the dir, which content you want to delete</param>
        protected virtual async Task DeleteDirContent(string absoluteDirPath)
        {
            var filesAndDirectoriesPattern = absoluteDirPath + @"\*";
            var storageFolder = await ISF.GetFolderAsync(filesAndDirectoriesPattern);
            await DeleteFolderContentsAsync(storageFolder);
        }

        public static async Task DeleteFolderContentsAsync(StorageFolder folder,
            StorageDeleteOption option = StorageDeleteOption.Default)
        {
            try
            {
                // Try to delete all files
                var files = await folder.GetFilesAsync();
                foreach (var file in files)
                {
                    try
                    {
                        await file.DeleteAsync(option);
                    }
                    catch
                    {

                    }
                }
                // Iterate through all subfolders
                var subFolders = await folder.GetFoldersAsync();
                foreach (var subFolder in subFolders)
                {
                    try
                    {
                        // Delete the contents
                        await DeleteFolderContentsAsync(subFolder, option);
                        // Delete the subfolder
                        await subFolder.DeleteAsync(option);
                    }
                    catch
                    {

                    }
                }
            }
            catch
            {

            }
        }
    }
}
