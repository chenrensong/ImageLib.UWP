using ImageLib.Helpers;
using Windows.Security.Cryptography.Core;

namespace ImageLib.Cache.Storage
{
    /// <summary>
    /// Using MD5 hash generator to generate cache file names
    /// </summary>
    public class MD5CacheGenerator : ICacheGenerator
    {
        public string GenerateCacheName(string url)
        {
            return AlgorithmHelper.ComputeHash(url, HashAlgorithmNames.Md5);
        }
    }
}
