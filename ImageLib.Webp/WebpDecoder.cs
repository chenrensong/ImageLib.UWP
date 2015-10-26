using ImageLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using WebpLib;

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

        public void Dispose()
        {
            //empty
        }

        public async Task<ImageSource> InitializeAsync(CoreDispatcher dispatcher, IRandomAccessStream streamSource, CancellationTokenSource cancellationTokenSource)
        {
            byte[] bytes = new byte[streamSource.Size];
            await streamSource.ReadAsync(bytes.AsBuffer(), (uint)streamSource.Size, InputStreamOptions.None).AsTask(cancellationTokenSource.Token);
            var imageSource = WebpCodec.DecodeRGBA(bytes);
            return imageSource;
        }

        public bool IsSupportedFileFormat(byte[] header)
        {
            var riff = Encoding.UTF8.GetString(header, 0, 4);
            if ("RIFF".Equals(riff))
            {
                var webp = Encoding.UTF8.GetString(header, 8, 4);
                if ("WEBP".Equals(webp))
                {
                    return true;
                }
            }
            return false;
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
