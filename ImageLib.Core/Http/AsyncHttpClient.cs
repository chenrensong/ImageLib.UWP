using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ImageLib.Http
{
    public class AsyncHttpClient
    {
        private Uri _uri;
        private Dictionary<string, string> _headers;
        private string _encoding;

        public AsyncHttpClient Url(string url)
        {
            _uri = new Uri(url);
            return this;
        }

        public AsyncHttpClient Uri(Uri uri)
        {
            _uri = uri;
            return this;
        }


        public AsyncHttpClient Encoding(string encoding)
        {
            _encoding = encoding;
            Header("Encoding", encoding);
            return this;
        }


        public AsyncHttpClient Header(string name, string value)
        {
            if (_headers == null)
            {
                _headers = new Dictionary<string, string>();
            }
            _headers[name] = value;

            return this;
        }

        public AsyncHttpClient Cookies(string cookies)
        {
            if (cookies != null)
            {
                Header("Cookie", cookies);
            }
            return this;
        }

        public AsyncHttpClient Referer(string referer)
        {
            if (referer != null)
            {
                Header("Referer", referer);
            }
            return this;
        }

        public AsyncHttpClient UserAgent(string userAgent)
        {
            if (userAgent != null)
            {
                Header("User-Agent", userAgent);
            }
            return this;
        }

        public AsyncHttpClient ContentType(string contentType)
        {
            if (contentType != null)
            {
                Header("Content-Type", contentType);
            }
            return this;
        }

        public AsyncHttpClient Accept(string accept)
        {
            if (accept != null)
            {
                Header("Accept", accept);
            }
            return this;
        }


        public async Task<AsyncHttpResponse> Get()
        {
            var client = DoBuildHttpClient();

            try
            {
                using (var rsp = await client.GetAsync(_uri))
                {
                    return new AsyncHttpResponse(rsp, _encoding);
                }
            }
            catch (Exception ex)
            {
                return new AsyncHttpResponse(ex, _encoding);
            }
        }

        public async Task<AsyncHttpResponse> Post(Dictionary<string, string> args)
        {
            var client = DoBuildHttpClient();

            var postData = new FormUrlEncodedContent(args);

            try
            {
                using (var rsp = await client.PostAsync(_uri, postData))
                {
                    return new AsyncHttpResponse(rsp, _encoding);
                }
            }
            catch (Exception ex)
            {
                return new AsyncHttpResponse(ex, _encoding);
            }
        }

        private HttpClient DoBuildHttpClient()
        {

            HttpClient client = new HttpClient();

            if (_headers != null)
            {
                foreach (var kv in _headers)
                {
                    client.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                }
            }

            return client;
        }
    }
}
