namespace ImageLib.Gif
{
    public class GifHelper
    {
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
