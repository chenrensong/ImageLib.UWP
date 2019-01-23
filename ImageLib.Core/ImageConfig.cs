using ImageLib.Cache;
using ImageLib.Cache.Storage;
using ImageLib.Cache.Storage.CacheImpl;
using ImageLib.Parser;
using ImageLib.Support;
using System;
using System.Collections.Generic;
using Windows.Storage;

namespace ImageLib
{
    public class ImageConfig
    {

        /// <summary>
        /// 默认Config
        /// </summary>
        internal static ImageConfig Default;
        internal static readonly Dictionary<string, ImageLoader> Collection = new Dictionary<string, ImageLoader>();

        public readonly CacheMode CacheMode;
        public readonly StorageCacheBase StorageCacheImpl;
        public readonly IUriParser UriParser;
        public readonly List<Type> DecoderTypes;
        public readonly bool NewApiSupported;
        /// <summary>
        /// Enable/Disable log output for ImageLoader
        /// Default - false
        /// </summary>
        public readonly bool IsLogEnabled;

        private ImageConfig(Builder builder)
        {
            if (builder.StorageCacheImpl == null)
            {
                CacheMode = CacheMode.NoCache;
            }
            else
            {
                CacheMode = CacheMode.OnlyStorageCache;
            }
            StorageCacheImpl = builder.StorageCacheImpl;
            DecoderTypes = builder.DecoderTypes;
            UriParser = builder.UriParser;
            NewApiSupported = builder.NewApiSupported;
        }

        /// <summary>
        /// Implements Builder pattern
        /// </summary>
        /// <see cref="http://en.wikipedia.org/wiki/Builder_pattern"/>
        public class Builder
        {
            /// <summary>
            /// Gets/Sets memory cache implementation for ImageLoader
            /// If you will leave it empty but CacheMode will require it, will be used WeakMemoryCache implementation 
            /// </summary>
            //public MemoryCacheBase<string, IRandomAccessStream> MemoryCacheImpl { get; set; }

            /// <summary>
            /// Gets/Sets storage cache implementation for ImageLoader
            /// If you will leave it empty but CacheMode will require it, exception will be thrown
            /// Default - null, I am sorry for that, but it requires StorageFolder instance, so you have to init it anyway
            /// </summary>
            internal StorageCacheBase StorageCacheImpl { get; private set; }

            internal IUriParser UriParser { get; private set; }

            internal bool NewApiSupported { get; private set; } = true;

            internal bool IsLogEnabled { get; private set; } = false;

            internal List<Type> DecoderTypes { get; private set; } = new List<Type>();

            public Builder AddDecoder<TDecoder>() where TDecoder : IImageDecoder
            {
                if (typeof(TDecoder) == typeof(DefaultDecoder))
                {
                    new ArgumentException("DefaultDecoder is default decoder");
                }

                if (!DecoderTypes.Contains(typeof(TDecoder)))
                {
                    DecoderTypes.Add(typeof(TDecoder));
                }
                return this;
            }

            public Builder NewApi(bool isSupported)
            {
                NewApiSupported = isSupported;
                return this;
            }

            /// <summary>
            /// 是否支持最新的Gif Api
            /// </summary>
            /// <param name="isSupported"></param>
            /// <returns></returns>
            public Builder LimitedStorageCache(StorageFolder isf, string cacheDirectory, ICacheGenerator cacheFileNameGenerator, long cacheLimitInBytes)
            {
                StorageCacheImpl = new LimitedStorageCache(isf, cacheDirectory, cacheFileNameGenerator, cacheLimitInBytes);
                return this;
            }

            /// <summary>
            /// 是否开启日志
            /// </summary>
            /// <param name="isLogEnabled"></param>
            /// <returns></returns>
            public Builder LogEnabled(bool isLogEnabled)
            {
                IsLogEnabled = isLogEnabled;
                return this;
            }


            public ImageConfig Build()
            {
                AddDecoder<DefaultDecoder>();//添加默认Decoder
                return new ImageConfig(this);
            }

        }
    }
}
