using System.Text;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace ImageLib.Helpers
{
    internal class AlgorithmHelper
    {
        /// <summary>
        /// Computes hash algorithm for the source string
        /// </summary>
        /// <param name="source">Source string to compute hash from</param>
        /// <param name="algorithm">HashAlgorithmNames.Sha1</param>
        /// <returns>hash from the source string</returns>
        public static string ComputeHash(string source, string algorithm)
        {
            HashAlgorithmProvider sha1 = HashAlgorithmProvider.OpenAlgorithm(algorithm);
            byte[] bytes = Encoding.UTF8.GetBytes(source);
            IBuffer bytesBuffer = CryptographicBuffer.CreateFromByteArray(bytes);
            IBuffer hashBuffer = sha1.HashData(bytesBuffer);
            return CryptographicBuffer.EncodeToHexString(hashBuffer);
        }
    }
}
