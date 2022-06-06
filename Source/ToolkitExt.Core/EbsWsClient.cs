// MIT License
// 
// Copyright (c) 2022 SirRandoo
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using WatsonWebsocket;

namespace ToolkitExt.Core
{
    /// <summary>
    ///     A client for connecting to the extension backend service.
    /// </summary>
    public class EbsWsClient
    {
        private static readonly Uri URL = new Uri("wss://ws-us3.pusher.com/app/290b2ad8d139f7d58165?protocol=7&client=js&version=7.0.6&flash=false");
        private readonly WatsonWsClient _webSocket;

        protected EbsWsClient()
        {
            _webSocket = new WatsonWsClient(URL);
            _webSocket.ServerConnected += OnConnected;
            _webSocket.ServerDisconnected += OnDisconnected;
        }

        [NotNull]
        public static EbsWsClient Instance { get; } = new EbsWsClient();

        /// <summary>
        ///     Whether the client is currently connected to the EBS.
        /// </summary>
        public bool IsConnected => _webSocket.Connected;

        private void OnDisconnected(object sender, EventArgs e)
        {
            // TODO: Log that the client disconnected.

            Task.Factory.StartNew(
                async () =>
                {
                    await ReconnectAsync();
                }
            );
        }

        private async Task ReconnectAsync()
        {
            // TODO: Log that the client is reconnecting.

            var tries = 0;

            while (tries < 5)
            {
                if (await ReconnectInternalAsync())
                {
                    break;
                }

                tries += 1;

                var initialBackoff = (int)Math.Pow(2, tries);
                var jitter = (int)Math.Ceiling(initialBackoff * 0.9);
                int finalBackoff = initialBackoff * new Random().Next(jitter);

                await Task.Delay(TimeSpan.FromSeconds(finalBackoff));
            }

            // TODO: Log the current state of the client after
        }

        private async Task<bool> ReconnectInternalAsync() => await ReconnectInternalAsync(CancellationToken.None);

        private async Task<bool> ReconnectInternalAsync(CancellationToken cancellationToken) => await _webSocket.StartWithTimeoutAsync(5, cancellationToken);

        private void OnConnected(object sender, EventArgs e)
        {
            // TODO: Log that the client connected to the EBS.
        }
    }
}
