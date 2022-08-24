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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using ToolkitExt.Api;
using ToolkitExt.Api.Enums;
using ToolkitExt.Api.Interfaces;
using ToolkitExt.Core.Entities;
using ToolkitExt.Core.Events;
using ToolkitExt.Core.Models;
using ToolkitExt.Core.Requests;
using ToolkitExt.Core.Responses;

namespace ToolkitExt.Core
{
    /// <summary>
    ///     A class for interacting with the extension backend service.
    /// </summary>
    public class BackendClient
    {
        private static readonly RimLogger Logger = new RimLogger("ToolkitClient");
        private readonly EbsHttpClient _httpClient = new EbsHttpClient();

        private readonly EbsWsClient _wsClient = new EbsWsClient();
        private volatile string _channelId;

        private BackendClient()
        {
            _wsClient.Subscribed += OnSubscribed;
            _wsClient.ConnectionEstablished += OnConnectionEstablished;
        }

        /// <summary>
        ///     The current instance of the backend client.
        /// </summary>
        /// <remarks>
        ///     Only one instance should be active at any given time
        /// </remarks>
        public static BackendClient Instance { get; } = new BackendClient();

        public bool Connected => _wsClient.IsConnected;

        /// <summary>
        ///     The current state of the websocket client.
        /// </summary>
        public ConnectionState WsState => _wsClient.State;
        
        public string ChannelId
        {
            get => _channelId;
            private set => _channelId = value;
        }

        public async Task ReconnectAsync()
        {
            await _wsClient.ReconnectAsync();
        }

        public void RegisterHandler(IWsMessageHandler handler) => _wsClient.RegisterHandler(handler);

        public void UnregisterHandler(Type handlerType)
        {
            foreach (IWsMessageHandler handler in _wsClient.Handlers)
            {
                if (handler.GetType() == handlerType)
                {
                    _wsClient.UnregisterHandler(handler);
                }
            }
        }

        /// <summary>
        ///     Sends a raw <see cref="PusherRequest"/> to the websocket backend.
        /// </summary>
        /// <param name="request">The request to send</param>
        /// <returns>Whether the request was sent</returns>
        public async Task<bool> PushRaw(PusherRequest request) => await _wsClient.Send(request);

        /// <summary>
        ///     Sets the credentials for the client.
        /// </summary>
        /// <param name="channelId">The channel id to use for certain requests</param>
        /// <param name="token">A token representing the broadcaster's channel</param>
        public async Task SetCredentials(string channelId, string token)
        {
            _httpClient.SetToken(token);
            ChannelId = channelId;

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
                if (choice == null)
                {
                    Logger.Debug("Encountered a null choice; aborting...");

                    return null;
                }
                
                request.AddOption(choice.Id.ToString(), choice.Label, choice.Tooltip);
            }

            return await _httpClient.CreatePollAsync(request);
        }

        /// <summary>
        ///     Retrieves the channel's poll settings from the backend service.
        /// </summary>
        [ItemCanBeNull]
        public async Task<PollSettingsResponse> GetPollSettings() => await _httpClient.GetPollSettingsAsync(ChannelId);

        [ItemCanBeNull] internal async Task<DeletePollResponse> DeletePoll() => await _httpClient.DeletePollAsync();

        public async Task<bool> UpdateIncidentsAsync([NotNull] List<IncidentItem> items) => await _httpClient.UpdateIncidentsAsync(items);

        public async Task<bool> ValidateQueuedPoll(int id, bool valid, string errorString) => await _httpClient.ValidateQueuedPollAsync(id, valid, errorString);

        public async Task<bool> DeleteQueuedPoll(int id) => await _httpClient.DeleteQueuedPollAsync(id);

        [ItemNotNull] public async Task<QueuedPollPaginator> GetQueuedPolls() => await _httpClient.GetQueuedPollsAsync(_channelId);

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
                        AuthResponse response = await _httpClient.RetrieveToken(socketId, ChannelId);

                        if (response == null)
                        {
                            return;
                        }

                        await ProcessAuthResponse(response);
                    }
                )
               .ConfigureAwait(true);
        }

        private async Task ProcessAuthResponse([NotNull] AuthResponse response)
        {
            await _wsClient.Send(
                new SubscribeRequest
                {
                    Event = PusherEvent.Subscribe, Data = new SubscribeRequest.SubscribeData { Channel = $"private-gameclient.{ChannelId}", Auth = response.Auth }
                }
            );
        }
    }
}
