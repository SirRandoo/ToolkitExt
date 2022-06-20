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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using ToolkitExt.Api;
using ToolkitExt.Api.Interfaces;
using ToolkitExt.Core.Events;
using ToolkitExt.Core.Requests;
using ToolkitExt.Core.Responses;

namespace ToolkitExt.Core
{
    public class BackendClient
    {
        private static readonly RimLogger Logger = new RimLogger("ToolkitClient");
        private readonly EbsHttpClient _httpClient = new EbsHttpClient();

        private readonly EbsWsClient _wsClient = new EbsWsClient();
        private volatile string _channelId;

        public event EventHandler<ViewerVotedEventArgs> ViewerVoted; 

        private BackendClient()
        {
            _wsClient.Subscribed += OnSubscribed;
            _wsClient.ViewerVoted += OnViewerVoted;
            _wsClient.ConnectionEstablished += OnConnectionEstablished;
        }
        
        private void OnViewerVoted(object sender, ViewerVotedEventArgs e)
        {
            ViewerVoted?.Invoke(this, e);
        }

        public static BackendClient Instance { get; } = new BackendClient();

        public async Task<bool> PushRaw(PusherRequest request) => await _wsClient.Send(request);

        public async Task SetCredentials(string channelId, string token)
        {
            _httpClient.SetToken(token);
            _channelId = channelId;

            if (_wsClient.IsConnected)
            {
                await _wsClient.DisconnectAsync();
            }

            await _wsClient.ConnectAsync();
        }

        [ItemCanBeNull]
        internal async Task<CreatePollResponse> SendPoll([NotNull] IPoll poll)
        {
            var request = new PollRequest { Length = (int)(poll.EndedAt - poll.StartedAt).TotalMinutes, Title = poll.Caption };

            foreach (IOption choice in poll.Options)
            {
                request.AddOption(choice.Id.ToString(), choice.Label, choice.Tooltip);
            }

            return await _httpClient.CreatePollAsync(request);
        }

        /// <summary>
        ///     Retrieves the channel's poll settings from the backend service.
        /// </summary>
        [ItemCanBeNull] public async Task<PollSettingsResponse> GetPollSettings() => await _httpClient.GetPollSettingsAsync(_channelId);

        [ItemCanBeNull] internal async Task<DeletePollResponse> DeletePoll() => await _httpClient.DeletePollAsync();

        private static void OnSubscribed(object sender, [NotNull] SubscribedEventArgs e)
        {
            Logger.Info($"Subscribed to the channel {e.ChannelId}");
        }

        private void OnConnectionEstablished(object sender, [NotNull] ConnectionEstablishedEventArgs e)
        {
            string socketId = e.SocketId;

            Task.Run(
                async () =>
                {
                    AuthResponse response = await _httpClient.RetrieveToken(socketId, _channelId);

                    if (response == null)
                    {
                        return;
                    }

                    await ProcessAuthResponse(response);
                }
            );
        }

        private async Task ProcessAuthResponse([NotNull] AuthResponse response)
        {
            await _wsClient.Send(new SubscribeRequest { Event = "pusher:subscribe", Data = new SubscribeRequest.SubscribeData { Channel = $"private-private.{_channelId}", Auth = response.Auth } });
        }
    }
}
