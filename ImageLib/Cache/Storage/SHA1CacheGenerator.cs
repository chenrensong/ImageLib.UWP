using System.Text;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace ImageLib.Cache.Storage
{
    /// <summary>
    /// Using SHA1 hash generator to generate cache file names
    /// </summary>
    public class SHA1CacheGenerator : ICacheGenerator
    {
        public string GenerateCacheName(string url)
        {
            return SHA1Helper.ComputeHash(url);
        }

        private static class SHA1Helper
        {
            //private const string SUFFIX = ".cache";
            /// <summary>
            /// Computes SHA1 hash for the source string
            /// SHA1 because there is no .NET implementation of MD5 in WP .NET platform :(
            /// </summary>
            /// <param name="source">Source string to compute hash from</param>
            /// <returns>SHA1 hash from the source string</returns>
            public static string ComputeHash(string source)
            {
                HashAlgorithmProvider sha1 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
                byte[] bytes = Encoding.UTF8.GetBytes(source);
                IBuffer bytesBuffer = CryptographicBuffer.CreateFromByteArray(bytes);
                IBuffer hashBuffer = sha1.HashData(bytesBuffer);
                return CryptographicBuffer.EncodeToHexString(hashBuffer);
            }

        }
    }
}
