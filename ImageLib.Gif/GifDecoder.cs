using ImageLib.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;

namespace ImageLib.IO
{
    public class GifDecoder : IImageDecoder
    {

        private const string leftProperty = "/imgdesc/Left";
        private const string topProperty = "/imgdesc/Top";
        private const string widthProperty = "/imgdesc/Width";
        private const string heightProperty = "/imgdesc/Height";
        private const string delayProperty = "/grctlext/Delay";
        private const string disposalProperty = "/grctlext/Disposal";
        /// <summary>
        /// 标准的gif速度
        /// </summary>
        private readonly TimeSpan DefaultDelay = new TimeSpan(0, 0, 0, 0, 100);
        /// <summary>
        /// Decoder 后缀
        /// </summary>
        private const string Extension = "gif";

        public int HeaderSize
        {
            get { return 6; }
        }

        public bool IsSupportedFileExtension(string extension)
        {
            Guard.NotNullOrEmpty(extension, "extension");
            return string.Equals(extension, Extension, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsSupportedFileFormat(byte[] header)
        {
            return ImageHelper.IsGif(header);
        }


        /// <summary>
        /// Gets the number of frames in the gif animation
        /// </summary>
        public uint FrameCount
        {
            get;
            private set;
        }



        public async Task DecodeAsync(ImagePresenter imagePresenter, IRandomAccessStream stream,
            CancellationToken token)
        {

            if (imagePresenter == null || stream == null)
            {
                return;
            }

            try
            {
                imagePresenter.Frames.Clear();

                var imageFrames = new List<ImageFrame>();
                // Get the GIF decoder, to perform the magic
                var decoder = await BitmapDecoder.CreateAsync(BitmapDecoder.GifDecoderId, stream).AsTask(token);

                // Now we know the number of frames
                FrameCount = decoder.FrameCount;

                imagePresenter.PixelWidth = (int)decoder.PixelWidth;
                imagePresenter.PixelHeight = (int)decoder.PixelHeight;

                //  Extract each frame and create a WriteableBitmap for each of these (store them in an internal list)
                for (uint frameIndex = 0; frameIndex < FrameCount; frameIndex++)
                {
                    var frame = await decoder.GetFrameAsync(frameIndex).AsTask(token);

                    var writeableBitmap = new WriteableBitmap((int)decoder.OrientedPixelWidth, (int)decoder.OrientedPixelHeight);

                    BitmapFrame bframe = await decoder.GetFrameAsync(frameIndex).AsTask(token);

                    TimeSpan delay = TimeSpan.Zero;

                    BitmapPropertySet bitmapPropertySet =
                        await bframe.BitmapProperties.GetPropertiesAsync(new List<string>()).AsTask(token);

                    if (bitmapPropertySet != null)
                    {
                        BitmapPropertySet delayPropertySet = await (bitmapPropertySet["/grctlext"].Value
                            as BitmapPropertiesView).GetPropertiesAsync(new List<string> { "/Delay", });

                        if (delayPropertySet != null)
                        {
                            delay = TimeSpan.FromSeconds(double.Parse(delayPropertySet["/Delay"].Value.ToString()) / 100.0);
                        }
                    }

                    if (delay.Equals(TimeSpan.Zero))
                    {
                        delay = DefaultDelay;
                    }

                    //  Extract the pixel data and fill the WriteableBitmap with them
                    var bitmapTransform = new BitmapTransform();
                    var pixelDataProvider = await frame.GetPixelDataAsync(BitmapPixelFormat.Bgra8,
                        decoder.BitmapAlphaMode, bitmapTransform,
                        ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage);
                    var pixels = pixelDataProvider.DetachPixelData();

                    using (var bitmapStream = writeableBitmap.PixelBuffer.AsStream())
                    {
                        bitmapStream.Write(pixels, 0, pixels.Length);
                    }

                    //  Finally we have a frame (WriteableBitmap) that can internally be stored.
                    imageFrames.Add(new ImageFrame(writeableBitmap, delay));
                }
                imagePresenter.Frames.Clear();
                imagePresenter.Frames.AddRange(imageFrames);
            }
            finally
            {
                try
                {
                    if (stream != null)
                    {
                        stream.Dispose();
                        stream = null;
                    }
                }
                catch
                {

                }
            }

        }
    }
}
