using System;

namespace ImageLib.Helpers
{
    internal class DateTimeHelper
    {
        private static readonly DateTime zeroDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Fully Java analog for System.currentTimeMillis()
        /// </summary>
        /// <returns></returns>
        public static long CurrentTimeMillis()
        {
            return ConvertDateTimeToMillis(DateTime.Now);
        }

        public static long ConvertDateTimeToMillis(DateTime dateTime)
        {
            return (long)(dateTime - zeroDateTime).TotalMilliseconds;
        }
    }
}
