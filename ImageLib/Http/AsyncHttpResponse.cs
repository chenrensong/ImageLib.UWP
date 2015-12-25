using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace ImageLib.Http
{
    public class AsyncHttpResponse : IDisposable
    {
        protected byte[] Data { get; private set; }

        public Dictionary<string, string> Headers { get; private set; }
        public string Cookies { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public Encoding Encoding { get; private set; }
        public Exception Exception { get; private set; }

        internal AsyncHttpResponse(HttpResponseMessage rsp, string encoding)
        {
            this.StatusCode = rsp.StatusCode;
            this.Encoding = Encoding.GetEncoding(encoding ?? "UTF-8");
            this.Headers = new Dictionary<string, string>();
            this.Cookies = null;
            this.Exception = null;
            Init(rsp);
        }

        internal AsyncHttpResponse(Exception exp, string encoding)
        {
            this.StatusCode = 0;
            this.Encoding = Encoding.GetEncoding(encoding ?? "UTF-8");
            this.Headers = new Dictionary<string, string>();
            this.Cookies = null;
            this.Exception = exp;
        }

        protected async void Init(HttpResponseMessage rsp)
        {
            if (rsp.StatusCode == HttpStatusCode.OK)
            {
                this.Data = await rsp.Content.ReadAsByteArrayAsync();
            }

            if (rsp.Headers != null)
            {
                foreach (var kv in rsp.Headers)
                {
                    if ("Set-Cookie".Equals(kv.Key))
                    {
                        Cookies = string.Join(";", kv.Value);
                    }

                    Headers[kv.Key] = string.Join(";", kv.Value);
                }
            }
        }

        public async Task<IRandomAccessStream> GetRandomStream()
        {
            if (Data == null)
            {
                return null;
            }
            else
            {
                var buffer = this.GetBuffer();
                InMemoryRandomAccessStream inStream = new InMemoryRandomAccessStream();
                DataWriter datawriter = new DataWriter(inStream.GetOutputStreamAt(0));
                datawriter.WriteBuffer(buffer, 0, buffer.Length);
                await datawriter.StoreAsync();
                return inStream;
            }
          
        }

        public IBuffer GetBuffer()
        {
            if (Data == null)
                return null;
            else
                return WindowsRuntimeBufferExtensions.AsBuffer(Data);
        }

        public byte[] GetBytes()
        {
            if (Data == null)
                return null;
            else
                return Data;
        }

        public string GetString()
        {
            if (Data == null)
                return null;
            else
                return Encoding.GetString(Data);
        }

        public void Dispose()
        {
            Data = null;
            Headers = null;
            Cookies = null;
            Encoding = null;
            Exception = null;
        }
    }
}
