﻿// MIT License
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
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using ToolkitExt.Api;
using ToolkitExt.Api.Enums;
using ToolkitExt.Api.Events;
using ToolkitExt.Api.Interfaces;
using ToolkitExt.Core.Events;
using ToolkitExt.Core.Requests;
using ToolkitExt.Core.Responses;
using Verse;
using WatsonWebsocket;

namespace ToolkitExt.Core
{
    /// <summary>
    ///     A client for connecting to the extension backend service.
    /// </summary>
    internal sealed class EbsWsClient
    {
        private static readonly RimLogger Logger = new RimLogger("ToolkitWs");
        private static readonly Uri URL = new Uri("wss://ws-us3.pusher.com/app/290b2ad8d139f7d58165?protocol=7&client=js&version=7.0.6&flash=false");
        private readonly List<IWsMessageHandler> _handlers = new List<IWsMessageHandler>();
        private readonly WatsonWsClient _webSocket;

        internal EbsWsClient()
        {
            _webSocket = new WatsonWsClient(URL);
            _webSocket.ServerConnected += OnConnected;
            _webSocket.ServerDisconnected += OnDisconnected;
            _webSocket.MessageReceived += OnMessageReceived;
        }


        [NotNull]
        public IEnumerable<IWsMessageHandler> Handlers
        {
            get
            {
                lock (_handlers)
                {
                    return _handlers.ListFullCopy();
                }
            }
        }

        /// <summary>
        ///     Invoked when a viewer votes on the extension.
        /// </summary>
        public bool IsConnected => _webSocket.Connected;

        public void RegisterHandler(IWsMessageHandler handler)
        {
            lock (_handlers)
            {
                _handlers.Add(handler);
            }
        }

        public void UnregisterHandler(IWsMessageHandler handler)
        {
            lock (_handlers)
            {
                _handlers.Remove(handler);
            }
        }

        /// <summary>
        ///     Invoked when the client connects to the backend service.
        /// </summary>
        internal event EventHandler<ConnectionEstablishedEventArgs> ConnectionEstablished;

        /// <summary>
        ///     Invoked when the client successfully subscribes to a channel on
        ///     the backend service.
        /// </summary>
        internal event EventHandler<SubscribedEventArgs> Subscribed;

        /// <summary>
        ///     Sends a request to the backend service.
        /// </summary>
        /// <param name="request">The request to send</param>
        /// <returns>Whether the message was successfully sent</returns>
        internal async Task<bool> Send(PusherRequest request)
        {
            if (_webSocket.Connected && Json.TrySerialize(request, out string result))
            {
                return await _webSocket.SendAsync(result);
            }

            return false;
        }

        private async Task ReconnectAsync()
        {
            Logger.Info("Disconnected from backend; reconnecting...");

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

            Logger.Info($"Connection status: {_webSocket.Connected} ({tries} tries)");
        }

        private async Task<bool> ReconnectInternalAsync() => await ReconnectInternalAsync(CancellationToken.None);

        private async Task<bool> ReconnectInternalAsync(CancellationToken cancellationToken) => await _webSocket.StartWithTimeoutAsync(5, cancellationToken);

        private static void OnConnected(object sender, EventArgs e)
        {
            Logger.Info("Connected to the backend service.");
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            Logger.Warn("Disconnected from the backend service.");

            Task.Factory.StartNew(
                async () =>
                {
                    await ReconnectAsync();
                }
            );
        }

        private void OnMessageReceived(object sender, [NotNull] MessageReceivedEventArgs e)
        {
            string content = Encoding.UTF8.GetString(e.Data);

            if (!Json.TryDeserialize(content, out PusherResponse baseEvent))
            {
                return;
            }

            switch (baseEvent.Event)
            {
                case PusherEvent.ConnectionEstablished when Json.TryDeserialize(content, out ConnectionEstablishedResponse ev):
                    OnConnectionEstablished(new ConnectionEstablishedEventArgs(ev.Data.SocketId, ev.Data.ActivityTimeout));

                    return;
                case PusherEvent.Subscribe when Json.TryDeserialize(content, out SubscriptionSucceededResponse ev):
                    OnSubscribed(new SubscribedEventArgs(ev.Channel));

                    return;
                case PusherEvent.SubscriptionSucceeded:
                    Logger.Info("Subscription succeeded!");

                    return;
                case PusherEvent.Ping:
                    Task.Run(async () => await Send(new PongRequest()));

                    return;
                default:
                    Task.Run(async () => await ProcessMessage(new WsMessageEventArgs(baseEvent.Event, content)));

                    return;
            }
        }

        private async Task ProcessMessage(WsMessageEventArgs args)
        {
            List<IWsMessageHandler> handlers;

            lock (_handlers)
            {
                handlers = _handlers.FindAll(h => h.Event == args.EventId);
            }
            
            handlers.SortBy(h => h.Priority);
            
            foreach (IWsMessageHandler handler in handlers)
            {
                if (await handler.Handle(args))
                {
                    break;
                }
            }
        }

        internal async Task DisconnectAsync()
        {
            await _webSocket.StopAsync();
        }

        internal async Task ConnectAsync()
        {
            await _webSocket.StartAsync();
        }

        private void OnConnectionEstablished(ConnectionEstablishedEventArgs e)
        {
            ConnectionEstablished?.Invoke(this, e);
        }

        private void OnSubscribed(SubscribedEventArgs e)
        {
            Subscribed?.Invoke(this, e);
        }
    }
}
