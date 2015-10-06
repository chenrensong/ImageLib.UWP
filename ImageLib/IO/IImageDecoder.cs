using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace ImageLib.IO
{
    public interface IImageDecoder
    {

        /// <summary>
        /// Gets the size of the header for this image type.
        /// </summary>
        /// <value>The size of the header.</value>
        int HeaderSize { get; }

        /// <summary>
        /// Indicates if the image decoder supports the specified
        /// file extension.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <returns>
        /// <c>true</c>, if the decoder supports the specified
        /// extensions; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="extension"/>
        /// is null (Nothing in Visual Basic).</exception>
        /// <exception cref="System.ArgumentException"><paramref name="extension"/> is a string
        /// of length zero or contains only blanks.</exception>
        bool IsSupportedFileExtension(string extension);

        /// <summary>
        /// Indicates if the image decoder supports the specified
        /// file header.
        /// </summary>
        /// <param name="header">The file header.</param>
        /// <returns>
        /// <c>true</c>, if the decoder supports the specified
        /// file header; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="header"/>
        /// is null (Nothing in Visual Basic).</exception>
        bool IsSupportedFileFormat(byte[] header);

        /// <summary>
        /// Decode Image
        /// </summary>
        /// <param name="imagePresenter"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        Task DecodeAsync(ImagePresenter imagePresenter, IRandomAccessStream stream, CancellationToken token);
    }
}
