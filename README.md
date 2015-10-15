# ImageLib.UWP
  支持Universal Windows Platform(UWP)，基于微软最新的2d图形加速引擎**Win2d**，支持gif、jpg、png等格式。
  同时支持实现[IImageDecoder](https://github.com/chenrensong/ImageLib.UWP/blob/master/ImageLib/IO/IImageDecoder.cs)接口来支持更多图片格式。
 
## 初始化
``` c#
    ImageConfig.Initialize(new ImageConfig.Builder()
            {
                CacheMode = ImageLib.Cache.CacheMode.MemoryAndStorageCache,
                IsLogEnabled = true,
                MemoryCacheImpl = new WeakMemoryCache<string, IRandomAccessStream>(),
                StorageCacheImpl = new LimitedStorageCache(ApplicationData.Current.LocalCacheFolder,
                "test", new SHA1CacheGenerator(), 1024 * 1024 * 1024)
            }.AddDecoder<GifDecoder>().Build());
```
## 初始化
``` xaml
 <Controls:ImageView 
                Margin="0,20"
                UriSource="ms-appx:///Images/2.gif"
                Stretch="None"
               />
```

##支持URI格式
  http:, https:, ms-appx:,ms-appdata:
##支持平台
  **Client:** Windows 10
  
  **Server:** Windows Server 2016 
  
  **Phone:**  Windows 10 
##开发工具
  Visual Studio 2015 
##Nuget
``` c#
PM> Install-Package ImageLib.UWP
```

