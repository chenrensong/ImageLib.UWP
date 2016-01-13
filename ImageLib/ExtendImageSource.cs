using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace ImageLib
{
    public struct ExtendImageSource
    {
        public ImageSource ImageSource { get; set; }

        public double PixelWidth { get; set; }

        public double PixelHeight { get; set; }
    }
}
