using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageLib.IO
{
    public class DefaultDecoder : IImageDecoder
    {
        public int HeaderSize
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// 默认的优先级最低
        /// </summary>
        public int Priority
        {
            get
            {
                return int.MinValue;
            }
        }

        public void Dispose()
        {
        }

        public async Task<ImagePackage> InitializeAsync(CoreDispatcher dispatcher, Image image, IRandomAccessStream streamSource, CancellationTokenSource cancellationTokenSource)
        {
            var bitmapImage = new BitmapImage();
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                image.Source = bitmapImage;
            });
            await bitmapImage.SetSourceAsync(streamSource).AsTask(cancellationTokenSource.Token);
            return new ImagePackage(this, bitmapImage, bitmapImage.PixelWidth, bitmapImage.PixelHeight);
        }

        public bool IsSupportedFileFormat(byte[] header)
        {
            return true;
        }

        public ImageSource RecreateSurfaces()
        {
            return null;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}
