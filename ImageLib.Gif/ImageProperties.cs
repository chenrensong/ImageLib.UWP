// ===============================================================================
// GifDecoder.cs
// ImageLib for UWP
// ===============================================================================
// Copyright (c) 陈仁松. 
// All rights reserved.
// ===============================================================================



namespace ImageLib.Gif
{
    public sealed partial class GifDecoder
    {
        #region Private struct declarations
        internal struct ImageProperties
        {
            public readonly int PixelWidth;
            public readonly int PixelHeight;
            public readonly bool IsAnimated;
            public readonly int LoopCount;

            public ImageProperties(int pixelWidth, int pixelHeight, bool isAnimated, int loopCount)
            {
                PixelWidth = pixelWidth;
                PixelHeight = pixelHeight;
                IsAnimated = isAnimated;
                LoopCount = loopCount;
            }
        }




        #endregion

    }
}
