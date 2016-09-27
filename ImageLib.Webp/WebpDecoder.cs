// ===============================================================================
// WebpDecoder.cs
// ImageLib for UWP
// ===============================================================================
// Copyright (c) 陈仁松. 
// All rights reserved.
// ===============================================================================

using ImageLib.IO;
using System;
using System.Threading.Tasks;
using System.Threading;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using System.Runtime.InteropServices.WindowsRuntime;
using WebpLib;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Controls;

namespace ImageLib.Webp
{
    public class WebpDecoder : IImageDecoder
    {

        public int HeaderSize
        {
            get
            {
                return 12;
            }
        }

        public int Priority
        {
            get
            {
                return 0;
            }
        }

        public void Dispose()
        {
            //empty
        }

        public async Task<ImagePackage> InitializeAsync(CoreDispatcher dispatcher, Image image, IRandomAccessStream streamSource,
             CancellationTokenSource cancellationTokenSource)
        {
            byte[] bytes = new byte[streamSource.Size];
            await streamSource.ReadAsync(bytes.AsBuffer(), (uint)streamSource.Size, InputStreamOptions.None).AsTask(cancellationTokenSource.Token);
            int width, height;
            WriteableBitmap writeableBitmap = null;
            if (WebpCodec.GetInfo(bytes, out width, out height))
            {
                writeableBitmap = new WriteableBitmap(width, height);
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                 {
                     image.Source = writeableBitmap;
                 });
                WebpCodec.Decode(writeableBitmap, bytes);
            }
            return new ImagePackage(this, writeableBitmap, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight);
        }


        public bool IsSupportedFileFormat(byte[] header)
        {
            return header != null && header.Length >= 12
                && header[0] == 'R' && header[1] == 'I' && header[2] == 'F' && header[3] == 'F'
                && header[8] == 'W' && header[9] == 'E' && header[10] == 'B' && header[11] == 'P';
        }

        public ImageSource RecreateSurfaces()
        {
            return null;
        }

        public void Start()
        {
            //empty
        }

        public void Stop()
        {
            //empty
        }
    }
}
