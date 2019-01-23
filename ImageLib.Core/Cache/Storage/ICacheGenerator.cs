
namespace ImageLib.Cache.Storage
{
    public interface ICacheGenerator
    {
        string GenerateCacheName(string url);
    }
}
