// ===============================================================================
// Extensions.cs
// ImageLib for UWP
// ===============================================================================
// Copyright (c) 陈仁松. 
// All rights reserved.
// ===============================================================================

using ImageLib.Http;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using Windows.Storage;
using Windows.Storage.Streams;

namespace ImageLib.Helpers
{
    /// <summary>
    /// A collection of simple helper extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// 判断是否是web scheme
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static bool IsWebScheme(this Uri uri)
        {
            return "http".Equals(uri.Scheme) || "https".Equals(uri.Scheme);
        }

        public static async Task<IRandomAccessStream> GetStreamFromUri(this Uri uri, CancellationToken cancellationToken)
        {

            switch (uri.Scheme)
            {
                case "ms-appx":
                case "ms-appdata":
                    {
                        var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
                        var result = await file.OpenAsync(FileAccessMode.Read).AsTask(cancellationToken).ConfigureAwait(false);
                        return result;
                    }
                case "ms-resource":
                    {
                        var rm = ResourceManager.Current;
                        var context = ResourceContext.GetForCurrentView();
                        var candidate = rm.MainResourceMap.GetValue(uri.LocalPath, context);
                        if (candidate != null && candidate.IsMatch)
                        {
                            var file = await candidate.GetValueAsFileAsync();
                            return await file.OpenAsync(FileAccessMode.Read).AsTask(cancellationToken).ConfigureAwait(false);
                        }
                        throw new Exception("Resource not found");
                    }
                case "file":
                    {
                        var file = await StorageFile.GetFileFromPathAsync(uri.LocalPath);
                        return await file.OpenAsync(FileAccessMode.Read).AsTask(cancellationToken).ConfigureAwait(false);
                    }
                case "http":
                case "https":
                    {
                        var httpClient = new AsyncHttpClient();
                        var rsp = await httpClient.Uri(uri).Get();
                        return await rsp.GetRandomStream();
                    }
                default:
                    {
                        try
                        {
                            RandomAccessStreamReference streamRef = RandomAccessStreamReference.CreateFromUri(uri);
                            return await streamRef.OpenReadAsync().AsTask(cancellationToken).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            return null;
                        }
                    }
            }
        }

    }
}
