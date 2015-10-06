using System;
using Windows.UI.Xaml.Media.Imaging;


namespace ImageLib
{
    public class ImageFrame
    {
        public ImageFrame(WriteableBitmap bitmap, TimeSpan delay)
        {
            this.BitmapFrame = bitmap;
            this.Delay = delay;
        }

        /// <summary>
        /// 图片帧
        /// </summary>
        public WriteableBitmap BitmapFrame { get; private set; }

        /// <summary>
        /// 图片帧速度
        /// </summary>
        public TimeSpan Delay { get; private set; }

    }
}
