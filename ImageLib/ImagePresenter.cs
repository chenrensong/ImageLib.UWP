using ImageLib.IO;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using Windows.UI.Xaml.Controls;
using System.Linq;
using System;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading;

namespace ImageLib
{
    public class ImagePresenter
    {

        /// <summary>
        /// Cancellation Token Source
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        private volatile List<ImageFrame> _frames = new List<ImageFrame>();


        /// <summary>
        /// 图片帧列表
        /// </summary>
        public List<ImageFrame> Frames
        {
            get
            {
                Contract.Ensures(Contract.Result<List<ImageFrame>>() != null);
                return _frames;
            }
        }

        /// <summary>
        ///高度(以像素为单位)
        /// </summary>
        public int PixelHeight { get; internal set; }

        /// <summary>
        ///宽度(以像素为单位)
        /// </summary>
        public int PixelWidth { get; internal set; }

     

        /// <summary>
        /// Build
        /// </summary>
        /// <param name="image"></param>
        /// <param name="animation"></param>
        /// <param name="isAutoPaly"></param>
        public async Task BuildAsync(Uri uri, Image image, ImageAnimation animation, bool isAutoPaly)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            if (uri == null)
            {
                return;
            }
            var res = await LoadStreamAsync(uri);
            if (res == null)
            {
                return;
            }
            bool hasDecoder = false;
            var decoders = Decoders.GetAvailableDecoders();
            if (decoders.Count > 0)
            {
                int maxHeaderSize = decoders.Max(x => x.HeaderSize);
                if (maxHeaderSize > 0)
                {
                    byte[] header = new byte[maxHeaderSize];
                    var resAsStream = res.AsStream();
                    resAsStream.Seek(0L, SeekOrigin.Begin);
                    resAsStream.Read(header, 0, maxHeaderSize);
                    resAsStream.Position = 0;
                    var decoder = decoders.FirstOrDefault(x => x.IsSupportedFileFormat(header));
                    if (decoder != null)
                    {
                        await decoder.DecodeAsync(this, res, _cancellationTokenSource.Token);
                        hasDecoder = true;
                    }
                }
            }

            //没有找到decoder，采用BitmapImage默认的解码
            if (!hasDecoder)
            {
                var bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(res).AsTask(_cancellationTokenSource.Token);
                this.PixelHeight = bitmapImage.PixelHeight;
                this.PixelWidth = bitmapImage.PixelWidth;
                image.Source = bitmapImage;
            }
            else
            {
                if (this.Frames.Count > 0)
                {
                    if (this.Frames.Count > 1)
                    {
                        animation.SetAnimation(image, this.Frames);
                        if (isAutoPaly)
                        {
                            animation.Begin();
                        }
                    }
                    else
                    {
                        image.Source = this.Frames[0].BitmapFrame;
                    }
                }
            }
        }

        public void Clear()
        {
            this._cancellationTokenSource?.Cancel();
            this._cancellationTokenSource = null;
            this.PixelHeight = 0;
            this.PixelWidth = 0;
            if (this.Frames.Count > 0)
            {
                this.Frames.Clear();
            }
        }

        private async Task<IRandomAccessStream> LoadStreamAsync(Uri uri)
        {
            if (uri == null)
            {
                return null;
            }
            if (uri.IsFile)
            {
                var storageFile = await StorageFile.GetFileFromApplicationUriAsync(uri);
                return await storageFile.OpenAsync(FileAccessMode.Read).AsTask(_cancellationTokenSource.Token);
            }
            else
            {
                RandomAccessStreamReference task = RandomAccessStreamReference.CreateFromUri(uri);
                return await task.OpenReadAsync().AsTask(_cancellationTokenSource.Token);
            }
        }

    }
}
