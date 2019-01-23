using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace ImageLib.Parser
{
    public interface IUriParser
    {
        Task<IRandomAccessStream> GetStreamFromUri(Uri uri, CancellationToken cancellationToken);
    }
}
