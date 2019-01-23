// ===============================================================================
// GifDecoder.cs
// ImageLib for UWP
// ===============================================================================
// Copyright (c) 陈仁松. 
// All rights reserved.
// ===============================================================================

using Windows.Foundation;


namespace ImageLib.Gif
{
    public sealed partial class GifDecoder
    {
        #region Private struct declarations

        internal struct FrameProperties
        {
            public readonly Rect Rect;
            public readonly double DelayMilliseconds;
            public readonly bool ShouldDispose;
            public readonly uint FrameIndex;

            public FrameProperties(uint frameIndex, Rect rect, double delayMilliseconds, bool shouldDispose)
            {
                FrameIndex = frameIndex;
                Rect = rect;
                DelayMilliseconds = delayMilliseconds;
                ShouldDispose = shouldDispose;
            }
        }




        #endregion

    }
}
