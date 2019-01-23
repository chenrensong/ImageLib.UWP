using System;
using System.Collections.Generic;
using Windows.Graphics.Imaging;

namespace ImageLib.Cache.Memory
{
    #region ImagingCache

    ///
    /// ImagingCache provides caching for different Imaging objects
    /// Caches are thread-safe.
    ///
    public static class ImagingCache
    {

        #region Methods

        private static void AddToCache(Uri uri, object obj)
        {
            AddToCache(uri, obj, _cache);
        }

        private static void RemoveFromCache(Uri uri)
        {
            RemoveFromCache(uri, _cache);
        }

        private static object CheckCache(Uri uri)
        {
            return CheckCache(uri, _cache);
        }

        public static void Remove(Uri uri)
        {
            RemoveFromCache(uri);
        }

        public static void Add(Uri uri, object obj)
        {
            AddToCache(uri, new WeakReference(obj), _cache);
        }

        public static T Get<T>(Uri uri) where T : class
        {
            if (uri != null)
            {
                WeakReference objectWeakReference = ImagingCache.CheckCache(uri) as WeakReference;
                if (objectWeakReference != null)
                {
                    T bitmapImage = (objectWeakReference.Target as T);
                    if (bitmapImage != null)
                    {
                        return bitmapImage;
                    }
                    else
                    {
                        ImagingCache.RemoveFromCache(uri);
                    }
                }
            }
            return null;
        }

        /// Adds an object to a given table
        private static void AddToCache(Uri uri, object obj, Dictionary<Uri, object> table)
        {
            lock (table)
            {
                // if entry is already there, exit
                if (table.ContainsKey(uri))
                {
                    return;
                }

                // if the table has reached the max size, try to see if we can reduce its size
                if (table.Count == MAX_CACHE_SIZE)
                {
                    var al = new List<Uri>();
                    foreach (var de in table)
                    {
                        // if the value is a WeakReference that has been GC'd, remove it
                        WeakReference weakRef = de.Value as WeakReference;
                        if ((weakRef != null) && (weakRef.Target == null))
                        {
                            al.Add(de.Key);
                        }
                    }
                    foreach (var o in al)
                    {
                        table.Remove(o);
                    }
                }
                // if table is still maxed out, exit
                if (table.Count == MAX_CACHE_SIZE)
                {
                    return;
                }
                // add it
                table[uri] = obj;
            }
        }

        /// Removes an object from a given table
        private static void RemoveFromCache(Uri uri, Dictionary<Uri, object> table)
        {
            lock (table)
            {
                // if entry is there, remove it
                if (table.ContainsKey(uri))
                {
                    table.Remove(uri);
                }
            }
        }

        /// Return an object from a given table
        private static object CheckCache(Uri uri, Dictionary<Uri, object> table)
        {
            lock (table)
            {
                if (table.ContainsKey(uri))
                {
                    return table[uri];
                }
                return null;
            }
        }

        #endregion

        #region Data Members

        /// decoder cache
        private static Dictionary<Uri, object> _cache = new Dictionary<Uri, object>();

        /// max size to limit the cache
        private static int MAX_CACHE_SIZE = 10;

        #endregion
    }

    #endregion
}
