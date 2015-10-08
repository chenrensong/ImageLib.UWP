// ===============================================================================
// LoadingCompletedEventArgs.cs
// ImageLib for UWP
// ===============================================================================
// Copyright (c) 陈仁松. 
// All rights reserved.
// ===============================================================================

using System;

namespace ImageLib.Controls
{
    public class LoadingCompletedEventArgs : EventArgs
    {
        public double PixelHeight
        {
            get;
            set;
        }
        public double PixelWidth
        {
            get;
            set;
        }
        internal LoadingCompletedEventArgs(double pixelWidth, double pixelHeight)
        {
            this.PixelHeight = pixelHeight;
            this.PixelWidth = pixelWidth;
        }
    }
}