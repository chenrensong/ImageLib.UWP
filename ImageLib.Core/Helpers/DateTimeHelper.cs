using System;

namespace ImageLib.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly DateTime ZeroDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Fully Java analog for System.currentTimeMillis()
        /// </summary>
        /// <returns></returns>
        public static long CurrentTimeMillis()
        {
            return DateTime.Now.Milliseconds();
        }

        public static long Milliseconds(this DateTime dateTime)
        {
            return (long)(dateTime - ZeroDateTime).TotalMilliseconds;
        }
    }
}
