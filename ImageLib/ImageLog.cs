// ===============================================================================
// ImageLog.cs
// ImageLib for UWP
// ===============================================================================
// Copyright (c) 陈仁松. 
// All rights reserved.
// ===============================================================================
using System.Diagnostics;

namespace ImageLib
{
    internal class ImageLog
    {
        private const string TAG = "[ImageLib]";

        /// <summary>
        /// Outputs log messages if IsLogEnabled
        /// </summary>
        /// <param name="message">to output</param>
        internal static void Log(string message)
        {
            if (ImageLoader.IsLogEnabled)
            {
                Debug.WriteLine("{0} {1}", TAG, message);
            }
        }
    }
}
