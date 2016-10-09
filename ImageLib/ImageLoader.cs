
using System;
using System.Threading.Tasks;
using ImageLib.Helpers;
using System.Threading;
using Windows.Storage.Streams;
using ImageLib.Cache;
using ImageLib.Schedulers;
using Windows.System.Threading;
using System.Collections.ObjectModel;
using ImageLib.IO;
using System.Collections.Generic;
using ImageLib.Cache.Memory;
using System.Linq;
using System.IO;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;

namespace ImageLib
{
    public class ImageLoader
    {
        /// <summary>
        /// Enable/Disable log output for ImageLoader
        /// Default - false
        /// </summary>
        internal static bool IsLogEnabled { get; set; }
        private static readonly object LockObject = new object();
        /// <summary>
        /// 默认的ImageLoader实例
        /// </summary>
        private static ImageLoader _instance;

        /// <summary>
        /// sequential Scheduler
        /// </summary>
        private TaskScheduler _sequentialScheduler;


        public static ImageLoader Initialize(ImageConfig imageConfig, bool isLogEnabled = false)
        {
            if (imageConfig == null)
            {
                throw new ArgumentException("Can not initialize ImageLoader with empty configuration");
            }
            if (ImageConfig.Default != null)
            {
                return ImageLoader.Instance;
            }
            IsLogEnabled = isLogEnabled;
            ImageConfig.Default = imageConfig;
            return ImageLoader.Instance;
        }

        /// <summary>
        /// 注册其他的Image Loader,便于不同策略使用
        /// </summary>
        /// <param name="key"></param>
        /// <param name="imageLoader"></param>
        public static void Register(string key, ImageConfig imageConfig)
        {
            ImageConfig.Collection.Add(key, new ImageLoader(imageConfig));
        }

        /// <summary>
        /// 默认的Image Loader
        /// </summary>
        public static ImageLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (LockObject)
                    {
                        if (_instance == null) _instance = new ImageLoader();
                    }
                }
                return _instance;
            }
        }


        protected ImageLoader()
        {
            _sequentialScheduler = new LimitedConcurrencyLevelTaskScheduler(1, WorkItemPriority.Normal, true);
        }

        protected ImageLoader(ImageConfig imageConfig) : this()
        {
            if (imageConfig == null)
            {
                throw new ArgumentException("Can not initialize ImageLoader with empty configuration");
            }
            ImageConfig.Default = imageConfig;
        }

        internal ReadOnlyCollection<IImageDecoder> GetAvailableDecoders()
        {
            List<IImageDecoder> decoders = new List<IImageDecoder>();
            foreach (Type decorderType in ImageConfig.Default.DecoderTypes)
            {
                if (decorderType != null)
                {
                    decoders.Add(Activator.CreateInstance(decorderType) as IImageDecoder);
                }
            }
            return new ReadOnlyCollection<IImageDecoder>(decoders);
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        /// <returns></returns>
        public virtual async Task ClearStorageCache()
        {
            CheckConfig();
            if (ImageConfig.Default.StorageCacheImpl != null)
            {
                await ImageConfig.Default.StorageCacheImpl.Clear();
            }
        }

        public virtual void ClearMemoryCache()
        {
            CheckConfig();
            if (ImageConfig.Default.MemoryCacheImpl != null)
            {
                ImageConfig.Default.MemoryCacheImpl.Clear();
            }
        }


        protected virtual void CheckConfig()
        {
            if (ImageConfig.Default == null)
            {
                throw new InvalidOperationException("ImageLoader configuration was not setted, please Initialize ImageLoader instance with JetImageLoaderConfiguration");
            }
        }

        /// <summary>
        /// Async loading image from cache or network
        /// </summary>
        /// <param name="imageUrl">Url of the image to load</param>
        /// <returns>BitmapImage if load was successfull or null otherwise</returns>
        public virtual async Task<ImagePackage> LoadImage(Image image, string imageUrl, CancellationTokenSource cancellationTokenSource)
        {
            return await LoadImage(image, new Uri(imageUrl), cancellationTokenSource);
        }


        /// <summary>
        /// Async loading image from cache or network
        /// </summary>
        /// <param name="imageUri">Uri of the image to load</param>
        /// <returns>BitmapImage if load was successfull or null otherwise</returns>
        public virtual async Task<ImagePackage> LoadImage(Image image, Uri uriSource,
            CancellationTokenSource cancellationTokenSource)
        {
            CheckConfig();
            ImagePackage imagePackage = null;
            var decoders = this.GetAvailableDecoders();
            var randStream = await this.LoadImageStream(uriSource, cancellationTokenSource);
            if (randStream == null)
            {
                throw new Exception("stream is null");
            }
            if (decoders.Count > 0)
            {
                int maxHeaderSize = decoders.Max(x => x.HeaderSize);
                if (maxHeaderSize > 0)
                {
                    byte[] header = new byte[maxHeaderSize];
                    var readStream = randStream.AsStreamForRead();
                    readStream.Position = 0;
                    await readStream.ReadAsync(header, 0, maxHeaderSize);
                    readStream.Position = 0;
                    int maxPriority = -1;
                    IImageDecoder decoder = null;
                    foreach (var item in decoders)
                    {
                        var priority = item.GetPriority(header);
                        if (priority > maxPriority)
                        {
                            maxPriority = priority;
                            decoder = item;
                        }
                    }
                    if (decoder != null)
                    {
                        var package = await decoder.InitializeAsync(image.Dispatcher, image, uriSource, randStream, cancellationTokenSource);
                        if (!cancellationTokenSource.IsCancellationRequested)
                        {
                            imagePackage = package;
                            //imagePackage?.Decoder?.Start();
                        }
                    }
                }
            }
            return imagePackage;

            //var bitmapImage = new BitmapImage();
            //var stream = await LoadImageStream(imageUri, cancellationTokenSource);
            //await bitmapImage.SetSourceAsync(stream);
            //return bitmapImage;
        }

        /// <summary>
        /// Async loading image stream from cache or network
        /// </summary>
        /// <param name="imageUri">Uri of the image to load</param>
        /// <returns>Stream of the image if load was successfull, null otherwise</returns>
        public virtual async Task<IRandomAccessStream> LoadImageStream(Uri imageUri,
            CancellationTokenSource cancellationTokenSource)
        {
            CheckConfig();
            if (imageUri == null)
            {
                return null;
            }
            var imageUrl = imageUri.AbsoluteUri;
            //有Cache情况，先加载Cache
            if (ImageConfig.Default.CacheMode != CacheMode.NoCache)
            {
                //加载Cache

                var resultFromCache = await this.LoadImageStreamFromCache(imageUri);
                if (resultFromCache != null)
                {
                    return resultFromCache;
                }
            }
            try
            {
                ImageLog.Log("[network] loading " + imageUrl);
                IRandomAccessStream randStream = null;
                //如果有自定义UriParser,使用自定义,反之使用默认方式.
                if (ImageConfig.Default.UriParser != null)
                {
                    randStream = await ImageConfig.Default.
                        UriParser.GetStreamFromUri(imageUri, cancellationTokenSource.Token);
                }
                else
                {
                    randStream = await imageUri.GetStreamFromUri(cancellationTokenSource.Token);
                }
                if (randStream == null)
                {
                    ImageLog.Log("[error] failed to download: " + imageUrl);
                    return null;
                }
                var inMemoryStream = new InMemoryRandomAccessStream();
                using (randStream)
                {
                    var copyAction = RandomAccessStream.CopyAndCloseAsync(
                              randStream.GetInputStreamAt(0L),
                              inMemoryStream.GetOutputStreamAt(0L));
                    await copyAction.AsTask(cancellationTokenSource.Token);
                }
                randStream = inMemoryStream;
                ImageLog.Log("[network] loaded " + imageUrl);
                if (ImageConfig.Default.CacheMode != CacheMode.NoCache)
                {
                    if (ImageConfig.Default.CacheMode == CacheMode.MemoryAndStorageCache ||
                         ImageConfig.Default.CacheMode == CacheMode.OnlyMemoryCache)
                    {
                        if (randStream != null)
                        {
                            ImageConfig.Default.MemoryCacheImpl.Put(imageUrl, randStream);
                        }
                    }

                    if (ImageConfig.Default.CacheMode == CacheMode.MemoryAndStorageCache ||
                     ImageConfig.Default.CacheMode == CacheMode.OnlyStorageCache)
                    {
                        //是http or https 才加入本地缓存
                        if (imageUri.IsWebScheme())
                        {
                            await Task.Factory.StartNew(() =>
                              {
                                  ImageLog.Log(string.Format("{0} in task t-{1}", imageUri, Task.CurrentId));
                                  // Async saving to the storage cache without await
                                  var saveAsync = ImageConfig.Default.StorageCacheImpl.SaveAsync(imageUrl, randStream)
                                        .ContinueWith(task =>
                                            {
                                                ImageLog.Log(string.Format("{0} in task t1-{1}", imageUri, Task.CurrentId));
                                                if (task.IsFaulted || !task.Result)
                                                {
                                                    ImageLog.Log("[error] failed to save in storage: " + imageUri);
                                                }

                                            }
                                    );
                              }, default(CancellationToken), TaskCreationOptions.AttachedToParent, this._sequentialScheduler);
                        }
                    }
                }

                return randStream;
            }
            catch (Exception ex)
            {
                ImageLog.Log("[error] failed to save loaded image: " + imageUrl);
            }

            //var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            // May be another thread has saved image to the cache
            // It is real working case
            if (ImageConfig.Default.CacheMode != CacheMode.NoCache)
            {
                var resultFromCache = await this.LoadImageStreamFromCache(imageUri);

                if (resultFromCache != null)
                {
                    return resultFromCache;
                }
            }

            ImageLog.Log("[error] failed to load image stream from cache and network: " + imageUrl);
            return null;
        }


        private async Task<IRandomAccessStream> LoadImageStreamFromCacheInternal(Uri imageUri)
        {

            var imageUrl = imageUri.AbsoluteUri;

            if (ImageConfig.Default.CacheMode == CacheMode.MemoryAndStorageCache ||
                ImageConfig.Default.CacheMode == CacheMode.OnlyMemoryCache)
            {
                IRandomAccessStream memoryStream;
                //尝试获取内存缓存
                if (ImageConfig.Default.MemoryCacheImpl.TryGetValue(imageUrl, out memoryStream))
                {
                    ImageLog.Log("[memory] " + imageUrl);
                    return memoryStream;
                }
            }

            //获取不到内存缓存
            if (ImageConfig.Default.CacheMode == CacheMode.MemoryAndStorageCache ||
                   ImageConfig.Default.CacheMode == CacheMode.OnlyStorageCache)
            {
                //网络uri且缓存可用
                if (imageUri.IsWebScheme() && await ImageConfig.Default.StorageCacheImpl.IsCacheExistsAndAlive(imageUrl))
                {
                    ImageLog.Log("[storage] " + imageUrl);
                    var storageStream = await ImageConfig.Default.StorageCacheImpl.LoadCacheStreamAsync(imageUrl);
                    // Moving cache to the memory
                    if (ImageConfig.Default.CacheMode == CacheMode.MemoryAndStorageCache
                          && storageStream != null)
                    {
                        ImageConfig.Default.MemoryCacheImpl.Put(imageUrl, storageStream);
                    }
                    return storageStream;
                }
            }
            return null;

        }


        /// <summary>
        /// Loads image stream from memory or storage cachecache
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <returns>Steam of the image or null if it was not found in cache</returns>
        protected virtual async Task<IRandomAccessStream> LoadImageStreamFromCache(Uri imageUri)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1000 * 10));
            return await Task<IRandomAccessStream>.Factory.StartNew(() =>
            {
                try
                {
                    var result = LoadImageStreamFromCacheInternal(imageUri).Result;
                    return result;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }, cts.Token);
        }


    }
}
