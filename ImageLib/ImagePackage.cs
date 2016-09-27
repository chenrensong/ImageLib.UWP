using ImageLib.IO;
using Windows.UI.Xaml.Media;

namespace ImageLib
{
    public class ImagePackage
    {
        public ImagePackage(IImageDecoder decoder, ImageSource source, double width, double height)
        {
            this.Decoder = decoder;
            this.ImageSource = source;
            this.PixelWidth = width;
            this.PixelHeight = height;
        }

        internal void UpdateSource(ImageSource source)
        {
            ImageSource = source;
        }

        public IImageDecoder Decoder { get; private set; }

        public ImageSource ImageSource { get; private set; }

        public double PixelWidth { get; private set; }

        public double PixelHeight { get; private set; }
    }
}
