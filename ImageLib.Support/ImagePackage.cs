using Windows.UI.Xaml.Media;

namespace ImageLib.Support
{
    public sealed class ImagePackage
    {
        public ImagePackage(IImageDecoder decoder, ImageSource source, double width, double height)
        {
            this.Decoder = decoder;
            this.ImageSource = source;
            this.PixelWidth = width;
            this.PixelHeight = height;
        }

        public void UpdateSource(ImageSource source)
        {
            ImageSource = source;
        }

        /// <summary>
        /// 与C++保留关键字冲突Dispose改为Release
        /// </summary>
        public void Release()
        {
            //Decoder?.Dispose();
            Decoder = null;
            ImageSource = null;
        }

        public IImageDecoder Decoder { get; private set; }

        public ImageSource ImageSource { get; private set; }

        public double PixelWidth { get; private set; }

        public double PixelHeight { get; private set; }
    }
}
