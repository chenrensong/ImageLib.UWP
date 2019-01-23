using ImageLib.Helpers;
using Windows.Security.Cryptography.Core;

namespace ImageLib.Cache.Storage
{
    /// <summary>
    /// Using SHA1 hash generator to generate cache file names
    /// </summary>
    public class SHA1CacheGenerator : ICacheGenerator
    {
        public string GenerateCacheName(string url)
        {
            return AlgorithmHelper.ComputeHash(url, HashAlgorithmNames.Sha1);
        }
    }
}
