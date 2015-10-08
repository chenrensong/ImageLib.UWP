using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;

namespace AnimatedGif
{
    public interface IImagePresenter
    {
        int HeaderSize { get; }

        void Start();

        void Stop();

        Task<ImageSource> InitializeAsync(IRandomAccessStream streamSource);

        ImageSource RecreateSurfaces();

        bool IsSupportedFileFormat(byte[] header);
    }
}
