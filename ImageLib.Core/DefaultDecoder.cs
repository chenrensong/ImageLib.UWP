using ImageLib.Support;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageLib
{
    public class DefaultDecoder : IImageDecoder, IDisposable
    {
        private static bool NewApiSupported = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Media.Imaging.BitmapImage", nameof(BitmapImage.IsAnimatedBitmap));
        private BitmapImage _bitmapImage;

        public int HeaderSize
        {
            get
            {
                return 33;
            }
        }

        public ImageSource RecreateSurfaces()
        {
            return null;
        }

        public void Dispose()
        {
            if (NewApiSupported)
            {
                if (_bitmapImage != null)
                {
                    if (_bitmapImage.IsAnimatedBitmap && _bitmapImage.IsPlaying)
                    {
                        _bitmapImage.Stop();
                    }
                }
            }
            _bitmapImage = null;
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

        public int GetPriority(IBuffer headerBuffer)
        {
            if (headerBuffer != null)
            {
                var isGifFormat = ImageFormat.IsGif(headerBuffer);
                if (isGifFormat && NewApiSupported && ImageConfig.Default.NewApiSupported)
                {
                    return int.MaxValue;//高优先级
                }
            }
            return 0;
        }

        public IAsyncOperation<ImagePackage> InitializeAsync(CoreDispatcher dispatcher, Image image, Uri uriSource, IRandomAccessStream streamSource)
        {
            _bitmapImage = new BitmapImage();
            return AsyncInfo.Run<ImagePackage>(
             async (token) =>
              {
                  await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                   {
                       var uri = image.Tag as Uri;
                       if (uri == uriSource)
                       {
                           image.Source = _bitmapImage;
                       }
                   }).AsTask().AsAsyncAction();
                  await _bitmapImage.SetSourceAsync(streamSource);
                  return new ImagePackage(this, _bitmapImage, _bitmapImage.PixelWidth, _bitmapImage.PixelHeight);
              });


        }
    }
}
