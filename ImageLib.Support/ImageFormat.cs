using Windows.Storage.Streams;

namespace ImageLib.Support
{
    public sealed class ImageFormat
    {
        public static bool IsGif(IBuffer buffer)
        {
            var headerBytes = new byte[6];
            using (var dataReader = DataReader.FromBuffer(buffer))
            {
                dataReader.ReadBytes(headerBytes);
            }
            var isGifFormat = headerBytes.Length >= 6 &&
               headerBytes[0] == 0x47 && // G
               headerBytes[1] == 0x49 && // I
               headerBytes[2] == 0x46 && // F
               headerBytes[3] == 0x38 && // 8
               (headerBytes[4] == 0x39 || headerBytes[4] == 0x37) && // 9 or 7
               headerBytes[5] == 0x61;   // a
            return isGifFormat;
        }

        public static bool IsWebP(IBuffer buffer)
        {
            var headerBytes = new byte[12];
            using (var dataReader = DataReader.FromBuffer(buffer))
            {
                dataReader.ReadBytes(headerBytes);
            }
            var isWebPFormat = headerBytes != null && headerBytes.Length >= 12
                && headerBytes[0] == 'R' && headerBytes[1] == 'I' && headerBytes[2] == 'F' && headerBytes[3] == 'F'
                && headerBytes[8] == 'W' && headerBytes[9] == 'E' && headerBytes[10] == 'B' && headerBytes[11] == 'P';
            return isWebPFormat;
        }


    }
}
