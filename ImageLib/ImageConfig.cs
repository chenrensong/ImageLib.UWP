
using System;
using ImageLib.Cache;
using ImageLib.Cache.Memory;
using ImageLib.Cache.Storage;
using Windows.Storage.Streams;
using System.Collections.Generic;
using ImageLib.IO;
using System.Collections.ObjectModel;

namespace ImageLib
{
    public class ImageConfig
    {
        internal static ImageConfig Config;

        /// <summary>
        /// Image decoder type list
        /// </summary>
        private static List<Type> _decoderTypes = new List<Type>();

        public static void Initialize(ImageConfig ImageLoaderConfig)
        {
            if (ImageLoaderConfig == null)
            {
                throw new ArgumentException("Can not initialize ImageLoader with empty configuration");
            }
            if (Config != null)
            {
                return;
            }
            Config = ImageLoaderConfig;
        }

        public static ReadOnlyCollection<IImageDecoder> GetAvailableDecoders()
        {
            List<IImageDecoder> decoders = new List<IImageDecoder>();
            foreach (Type decorderType in _decoderTypes)
            {
                if (decorderType != null)
                {
                    decoders.Add(Activator.CreateInstance(decorderType) as IImageDecoder);
                }
            }
            return new ReadOnlyCollection<IImageDecoder>(decoders);
        }

        public readonly bool IsLogEnabled;
        public readonly CacheMode CacheMode;
        public readonly MemoryCacheBase<string, IRandomAccessStream> MemoryCacheImpl;
        public readonly StorageCacheBase StorageCacheImpl;

        private ImageConfig(Builder builder)
        {
            IsLogEnabled = builder.IsLogEnabled;
            CacheMode = builder.CacheMode;
            MemoryCacheImpl = builder.MemoryCacheImpl;
            StorageCacheImpl = builder.StorageCacheImpl;
        }


        /// <summary>
        /// Implements Builder pattern
        /// </summary>
        /// <see cref="http://en.wikipedia.org/wiki/Builder_pattern"/>
        public class Builder
        {
            /// <summary>
            /// Enable/Disable log output for ImageLoader
            /// Default - false
            /// </summary>
            public bool IsLogEnabled { get; set; }


            /// <summary>
            /// Cache Mode
            /// </summary>
            private CacheMode _cacheMode = CacheMode.MemoryAndStorageCache;

            /// <summary>
            /// Gets/Sets caching mode for ImageLoader
            /// Default - CacheMode.MemoryAndStorageCache
            /// </summary>
            public CacheMode CacheMode { get { return _cacheMode; } set { _cacheMode = value; } }

            /// <summary>
            /// Gets/Sets memory cache implementation for ImageLoader
            /// If you will leave it empty but CacheMode will require it, will be used WeakMemoryCache implementation 
            /// </summary>
            public MemoryCacheBase<string, IRandomAccessStream> MemoryCacheImpl { get; set; }

            /// <summary>
            /// Gets/Sets storage cache implementation for ImageLoader
            /// If you will leave it empty but CacheMode will require it, exception will be thrown
            /// Default - null, I am sorry for that, but it requires StorageFolder instance, so you have to init it anyway
            /// </summary>
            public StorageCacheBase StorageCacheImpl { get; set; }


            public Builder AddDecoder<TDecoder>() where TDecoder : IImageDecoder
            {
                if (!_decoderTypes.Contains(typeof(TDecoder)))
                {
                    _decoderTypes.Add(typeof(TDecoder));
                }
                return this;
            }

            public ImageConfig Build()
            {
                CheckParams();
                return new ImageConfig(this);
            }

            private void CheckParams()
            {
                if ((CacheMode == CacheMode.MemoryAndStorageCache ||
                    CacheMode == CacheMode.OnlyMemoryCache) && MemoryCacheImpl == null)
                {
                    throw new ArgumentException("CacheMode " + CacheMode + " requires MemoryCacheImpl");
                }
                if ((CacheMode == CacheMode.MemoryAndStorageCache ||
                    CacheMode == CacheMode.OnlyStorageCache) && StorageCacheImpl == null)
                {
                    throw new ArgumentException("CacheMode " + CacheMode + " requires StorageCacheImpl");
                }
            }
        }
    }
}
