/*
 * The MIT License (MIT)
 * 
 * Copyright (c) 2021 NekomimiDaimao
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 * https://gist.github.com/nekomimi-daimao/e5726cde473de30a12273cd827779704
 * 
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Nekomimi.Daimao
{
    public sealed class EasyHttpRPC
    {
        private HttpListener _httpListener;
        public bool IsListening => _httpListener != null && _httpListener.IsListening;


        /// <summary>
        /// base url. if closed, "Closed"
        /// </summary>
        public string Address => IsListening ? _address : "Closed";

        private readonly string _address = "Closed";
        private const int PortDefault = 1234;

        /// <summary>
        /// post, this key
        /// </summary>
        public const string PostKey = "post";

        public EasyHttpRPC(CancellationToken cancellationToken, int port = PortDefault)
        {
            if (!HttpListener.IsSupported)
            {
                return;
            }

            _address = $"http://{IpAddress()}:{port}/";

            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add($@"http://+:{port}/");
            _httpListener.Start();

            ListeningLoop(_httpListener, cancellationToken).Forget();
        }

        public void Close()
        {
            _httpListener?.Close();
            _httpListener = null;
        }

        public static string IpAddress()
        {
            return Dns.GetHostAddresses(Dns.GetHostName())
                .Select(address => address.ToString())
                .FirstOrDefault(s => s.StartsWith("192.168"));
        }

        private readonly Dictionary<string, Func<NameValueCollection, UniTask<string>>> _functionsString =
            new Dictionary<string, Func<NameValueCollection, UniTask<string>>>();

        private readonly Dictionary<string, Func<NameValueCollection, Stream, UniTask<HttpStatusCode>>> _functionsFile =
            new Dictionary<string, Func<NameValueCollection, Stream, UniTask<HttpStatusCode>>>();

        public string[] Registered => _functionsString.Keys.Concat(_functionsFile.Keys).ToArray();

        /// <summary>
        /// Register function.
        /// <see cref="Func<NameValueCollection, UniTask<string>>"/>
        /// 
        /// <code>
        /// private async UniTask<string> Example(NameValueCollection arg)
        /// {
        ///     var example = arg["example"];
        ///     var result = await SomethingAsync(example);
        ///     return result;
        /// }
        /// </code>
        /// </summary>
        /// <param name="method">treated as lowercase</param>
        /// <param name="func"><see cref="Func<NameValueCollection, UniTask<string>>"/></param>
        /// 
        public void RegisterRPC(string method, Func<NameValueCollection, UniTask<string>> func)
        {
            _functionsString[method.ToLower()] = func;
        }

        public void RegisterRPC(string method, Func<NameValueCollection, Stream, UniTask<HttpStatusCode>> func)
        {
            _functionsFile[method.ToLower()] = func;
        }

        /// <summary>
        /// unregister function
        /// </summary>
        /// <param name="method"></param>
        public void UnregisterRPC(string method)
        {
            _functionsString.Remove(method);
            _functionsFile.Remove(method);
        }

        private async UniTaskVoid ListeningLoop(HttpListener listener, CancellationToken token)
        {
            token.Register(() => { listener?.Close(); });

            await UniTask.SwitchToThreadPool();

            while (true)
            {
                if (token.IsCancellationRequested || !listener.IsListening)
                {
                    break;
                }

                HttpListenerResponse response = null;

                try
                {
                    await UniTask.SwitchToThreadPool();
                    var context = await listener.GetContextAsync();
                    var request = context.Request;
                    response = context.Response;
                    response.ContentEncoding = Encoding.UTF8;

                    var method = request.RawUrl?.Split('?')[0].Remove(0, 1).ToLower();

                    if (method == null)
                    {
                        response.StatusCode = (int)HttpStatusCode.NotImplemented;
                        continue;
                    }

                    NameValueCollection nv = null;
                    if (string.Equals(request.HttpMethod, HttpMethod.Get.Method))
                    {
                        nv = request.QueryString;
                    }
                    else if (string.Equals(request.HttpMethod, HttpMethod.Post.Method))
                    {
                        string content;
                        using (var reader = new StreamReader(request.InputStream))
                        {
                            content = await reader.ReadToEndAsync();
                        }
                        nv = new NameValueCollection { [PostKey] = content };
                    }

                    if (_functionsString.TryGetValue(method, out var funcString))
                    {
                        await ParseString(nv, response, funcString);
                    }
                    else if (_functionsFile.TryGetValue(method, out var funcFile))
                    {
                        await ParseFile(nv, response, funcFile);
                    }
                    else
                    {
                        response.StatusCode = (int)HttpStatusCode.NotImplemented;
                        response.Close();
                    }
                }
                finally
                {
                    response?.Close();
                }
            }
        }

        private static async UniTask ParseString(
            NameValueCollection args,
            HttpListenerResponse response,
            Func<NameValueCollection, UniTask<string>> func)
        {
            try
            {
                var statusCode = HttpStatusCode.InternalServerError;
                string message;

                try
                {
                    message = await func(args);
                    statusCode = HttpStatusCode.OK;
                }
                catch (Exception e)
                {
                    message = e.Message;
                }

                response.StatusCode = (int)statusCode;
                using (var streamWriter = new StreamWriter(response.OutputStream))
                {
                    await streamWriter.WriteAsync(message);
                }
            }
            finally
            {
                response?.Close();
            }
        }

        private static async UniTask ParseFile(
            NameValueCollection args,
            HttpListenerResponse response,
            Func<NameValueCollection, Stream, UniTask<HttpStatusCode>> func)
        {
            try
            {
                var statusCode = HttpStatusCode.InternalServerError;
                try
                {
                    statusCode = await func(args, response.OutputStream);
                }
                catch (Exception e)
                {
                    // NOP
                }
                response.StatusCode = (int)statusCode;
            }
            finally
            {
                response?.Close();
            }
        }
    }
}
