using System.IO;


namespace ImageLib.Helpers
{
    public class ImageHelper
    {

        public static bool IsGif(Windows.Storage.Streams.IRandomAccessStream res)
        {
            int maxHeaderSize = 6;
            byte[] header = new byte[maxHeaderSize];
            var resAsStream = res.AsStream();
            resAsStream.Seek(0L, SeekOrigin.Begin);
            resAsStream.Read(header, 0, maxHeaderSize);
            resAsStream.Position = 0;
            return IsGif(header);
        }

        public static bool IsGif(byte[] header)
        {
            bool isGif = false;

            if (header.Length >= 6)
            {
                isGif =
                    header[0] == 0x47 && // G
                    header[1] == 0x49 && // I
                    header[2] == 0x46 && // F
                    header[3] == 0x38 && // 8
                   (header[4] == 0x39 || header[4] == 0x37) && // 9 or 7
                    header[5] == 0x61;   // a
            }

            return isGif;
        }
    }
}
