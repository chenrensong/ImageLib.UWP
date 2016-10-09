using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageLib.IO
{
    public class DefaultDecoder : IImageDecoder
    {
        private static bool NewApiSupported = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Media.Imaging.BitmapImage", nameof(BitmapImage.IsAnimatedBitmap));
        private BitmapImage _bitmapImage;
        private bool _isGifFormat = false;

        public int HeaderSize
        {
            get
            {
                return 33;
            }
        }

        public async Task<ImagePackage> InitializeAsync(CoreDispatcher dispatcher, Image image, Uri uriSource,
            IRandomAccessStream streamSource, CancellationTokenSource cancellationTokenSource)
        {
            _bitmapImage = new BitmapImage();
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var uri = image.Tag as Uri;
                if (uri == uriSource)
                {
                    image.Source = _bitmapImage;
                }
            });
            await _bitmapImage.SetSourceAsync(streamSource).AsTask(cancellationTokenSource.Token);
            return new ImagePackage(this, _bitmapImage, _bitmapImage.PixelWidth, _bitmapImage.PixelHeight);
        }

        public int GetPriority(byte[] header)
        {
            _isGifFormat = header != null && header.Length >= 6 &&
                    header[0] == 0x47 && // G
                    header[1] == 0x49 && // I
                    header[2] == 0x46 && // F
                    header[3] == 0x38 && // 8
                    (header[4] == 0x39 || header[4] == 0x37) && // 9 or 7
                    header[5] == 0x61;   // a
            if (_isGifFormat && NewApiSupported && ImageConfig.Default.NewApiSupported)
            {
                return int.MaxValue;//高优先级
            }
            return 0;
        }


        public ImageSource RecreateSurfaces()
        {
            return null;
        }

        public void Dispose()
        {
        }

        public void Start()
        {
            if (NewApiSupported)
            {
                _bitmapImage.Play();
            }
        }

        public void Stop()
        {
            if (NewApiSupported)
            {
                _bitmapImage.Stop();
            }
        }
    }
}
