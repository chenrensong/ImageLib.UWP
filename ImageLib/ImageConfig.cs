using System;
using ImageLib.Cache;
using ImageLib.Cache.Memory;
using ImageLib.Cache.Storage;
using Windows.Storage.Streams;
using System.Collections.Generic;
using ImageLib.IO;
using ImageLib.Parser;

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
        public readonly MemoryCacheBase<string, IRandomAccessStream> MemoryCacheImpl;
        public readonly StorageCacheBase StorageCacheImpl;
        public readonly IUriParser UriParser;
        public readonly List<Type> DecoderTypes;
        public readonly bool NewApiSupported;
 
        private ImageConfig(Builder builder)
        {
            if (builder.MemoryCacheImpl == null && builder.StorageCacheImpl == null)
            {
                CacheMode = CacheMode.NoCache;
            }
            else if (builder.MemoryCacheImpl == null && builder.StorageCacheImpl != null)
            {
                CacheMode = CacheMode.OnlyStorageCache;
            }
            else if (builder.MemoryCacheImpl != null && builder.StorageCacheImpl == null)
            {
                CacheMode = CacheMode.OnlyMemoryCache;
            }
            else
            {
                CacheMode = CacheMode.MemoryAndStorageCache;
            }
            MemoryCacheImpl = builder.MemoryCacheImpl;
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
            /// Decoder Types
            /// </summary>
            private readonly List<Type> _decoderTypes = new List<Type>();

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

            public IUriParser UriParser { get; set; }

            internal bool NewApiSupported { get; private set; } = true;

            public List<Type> DecoderTypes
            {
                get
                {
                    return _decoderTypes;
                }
            }

            public Builder AddDecoder<TDecoder>() where TDecoder : IImageDecoder
            {
                if (typeof(TDecoder) == typeof(DefaultDecoder))
                {
                    new ArgumentException("DefaultDecoder is default decoder");
                }

                if (!_decoderTypes.Contains(typeof(TDecoder)))
                {
                    _decoderTypes.Add(typeof(TDecoder));
                }
                return this;
            }

            public Builder NewApi(bool isSupported)
            {
                NewApiSupported = isSupported;
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
