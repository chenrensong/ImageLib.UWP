// ===============================================================================
// IImageDecoder.cs
// ImageLib for UWP
// ===============================================================================
// Copyright (c) 陈仁松. 
// All rights reserved.
// ===============================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ImageLib.IO
{
    public interface IImageDecoder : IDisposable
    {

        /// <summary>
        /// Gets the size of the header for this image type.
        /// </summary>
        /// <value>The size of the header.</value>
        int HeaderSize { get; }

        /// <summary>
        /// 越高优先级越高 -1 为不支持
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        int GetPriority(byte[] header);

        /// <summary>
        /// 执行播放(动态图片)
        /// </summary>
        void Start();

        /// <summary>
        /// 执行停止(动态图片)
        /// </summary>
        void Stop();

        /// <summary>
        /// Recreate Surfaces
        /// </summary>
        /// <returns></returns>
        ImageSource RecreateSurfaces();

        /// <summary>
        /// 异步初始化
        /// </summary>
        /// <param name="dispatcher">用于UI线程绘制</param>
        /// <param name="streamSource">stream</param>
        /// <returns></returns>
        Task<ImagePackage> InitializeAsync(CoreDispatcher dispatcher, Image image, Uri uriSource,
            IRandomAccessStream streamSource, CancellationTokenSource cancellationTokenSource);

    }
}
