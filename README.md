# ImageLib.UWP


[![NuGet Version](https://img.shields.io/nuget/v/imagelib.uwp.svg?style=flat)](https://www.nuget.org/packages?q=imagelib.uwp) 


  支持Universal Windows Platform(UWP)，基于微软最新的2d图形加速引擎**Win2d**，支持gif、jpg、png、webp等格式。
  同时支持实现[IImageDecoder](https://github.com/chenrensong/ImageLib.UWP/blob/master/ImageLib/IO/IImageDecoder.cs)接口来支持更多图片格式。
 
## 初始化
``` c#
  ImageLoader.Initialize(new ImageConfig.Builder()
            {
                CacheMode = ImageLib.Cache.CacheMode.MemoryAndStorageCache,
                MemoryCacheImpl = new LRUCache<string, IRandomAccessStream>(),
                StorageCacheImpl = new LimitedStorageCache(ApplicationData.Current.LocalCacheFolder,
                "cache", new SHA1CacheGenerator(), 1024 * 1024 * 1024)
            }.AddDecoder<GifDecoder>().AddDecoder<WebpDecoder>().Build(), false);
```
## XAML代码
``` xaml
 <controls:ImageView 
           Margin="0,20"
           UriSource="ms-appx:///Images/2.gif"
           Stretch="None"/>
```
## 自定义ImageLoader
``` c#
  ImageLoader.Register("test", new ImageConfig.Builder()
            {
                CacheMode = ImageLib.Cache.CacheMode.MemoryAndStorageCache,
                MemoryCacheImpl = new LRUCache<string, IRandomAccessStream>(),
                StorageCacheImpl = new LimitedStorageCache(ApplicationData.Current.LocalFolder,
                "cache1", new SHA1CacheGenerator(), 1024 * 1024 * 1024)
            }.AddDecoder<GifDecoder>().AddDecoder<WebpDecoder>().Build());
```
``` xaml
 <controls:ImageView 
           ImageLoaderKey="test"
           UriSource="ms-appx:///Images/2.gif"
           Stretch="None"/>
```
##支持URI格式
  http:, https:, ms-appx:,ms-appdata:,ms-resource;
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

