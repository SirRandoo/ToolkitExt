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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using RestSharp;
using ToolkitExt.Api;
using ToolkitExt.Core.Entities;
using ToolkitExt.Core.Models;
using ToolkitExt.Core.Requests;
using ToolkitExt.Core.Responses;
using ToolkitExt.Core.Serialization;

namespace ToolkitExt.Core
{
    internal class EbsHttpClient
    {
        private static readonly RimLogger Logger = new RimLogger("ToolkitHttp");
        private readonly RestClient _client = new RestClient(SiteMap.ApiBase);
        private volatile string _token;

        protected internal EbsHttpClient()
        {
            _client.UseSerializer<RestJsonSerializer>();
        }

        internal void SetToken(string token)
        {
            _token = token;
        }

        [NotNull]
        private RestRequest GetRequest([NotNull] string endpoint, Method method)
        {
            var request = new RestRequest(endpoint, method, DataFormat.Json);

            if (!string.IsNullOrEmpty(_token))
            {
                request.AddHeader("Authorization", $"Bearer {_token}");
            }

            return request;
        }

        [ItemCanBeNull]
        internal async Task<AuthResponse> RetrieveToken(string socketId, string channel)
        {
            RestRequest request = GetRequest("/broadcasting/auth", Method.POST);
            request.AddJsonBody(new BroadcastTokenRequest { ChannelName = $"private-gameclient.{channel}", SocketId = socketId });

            IRestResponse response = await _client.ExecuteAsync(request);

            return ResolveContent(response.Content, out AuthResponse data) ? data : null;
        }

        [ItemCanBeNull]
        internal async Task<CreatePollResponse> CreatePollAsync([NotNull] PollRequest poll)
        {
            RestRequest request = GetRequest("/broadcasting/polls/create", Method.POST);
            request.AddJsonBody(poll);

            IRestResponse response = await _client.ExecuteAsync(request);

            return ResolveContent(response.Content, out CreatePollResponse data) ? data : null;
        }

        [ItemCanBeNull]
        internal async Task<DeletePollResponse> DeletePollAsync()
        {
            RestRequest request = GetRequest("/broadcasting/polls/delete", Method.DELETE);
            IRestResponse<DeletePollResponse> response = await _client.ExecuteAsync<DeletePollResponse>(request);

            return ResolveContent(response.Content, out DeletePollResponse data) ? data : null;
        }

        [ItemCanBeNull]
        internal async Task<PollSettingsResponse> GetPollSettingsAsync(string id)
        {
            RestRequest request = GetRequest($"/settings/polls/{id}", Method.GET);
            IRestResponse response = await _client.ExecuteAsync(request);

            return ResolveContent(response.Content, out PollSettingsResponse data) ? data : null;
        }

        internal async Task<bool> UpdateIncidentsAsync([NotNull] List<IncidentItem> items)
        {
            RestRequest request = GetRequest("/initialize/incident-defs/update", Method.POST);
            request.AddJsonBody(items);

            IRestResponse response = await _client.ExecuteAsync(request);

            return ResolveContent(response.Content, out SuccessResponse _);
        }

        internal async Task<bool> ValidateQueuedPollAsync(int id, bool valid, string errorString)
        {
            RestRequest request = GetRequest($"/broadcasting/polls/queue/update/{id}", Method.POST);
            request.AddJsonBody(new[] { new QueuedPollValidatedRequest { Validated = valid, ValidationError = errorString } });

            IRestResponse response = await _client.ExecuteAsync(request);

            return ResolveContent(response.Content, out SuccessResponse _);
        }

        internal async Task<bool> DeleteQueuedPollAsync(int id)
        {
            RestRequest request = GetRequest($"/broadcasting/polls/queue/delete/{id}", Method.DELETE);
            IRestResponse response = await _client.ExecuteAsync(request);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return true;
                default:
                    return false;
            }
        }

        [ItemNotNull]
        internal Task<QueuedPollPaginator> GetQueuedPollsAsync(string channelId)
        {
            return Task.FromResult(new QueuedPollPaginator(this, channelId));
        }

        [ItemCanBeNull]
        internal async Task<GetQueuedPollsResponse> GetQueuedPollsAsync([NotNull] string channelId, int page)
        {
            IRestRequest request = GetRequest($"/broadcasting/polls/queue/index/{channelId}", Method.GET);
            request.AddQueryParameter("page", page.ToString());
            request.AddUrlSegment("channelId", channelId, false);

            IRestResponse response = await _client.ExecuteAsync(request, Method.GET);

            return ResolveContent(response.Content, out GetQueuedPollsResponse data) ? data : null;
        }

        [ContractAnnotation("=> true, response: notnull; => false, response: null")]
        private static bool ResolveContent<T>([NotNull] string content, out T response)
        {
            try
            {
                response = Json.Deserialize<T>(content);

                return true;
            }
            catch (JsonException)
            {
                if (!TryProcessError(content))
                {
                    Logger.Warn($"Could not deserialize response into {typeof(T)}; Raw response: {content}");
                }
            }

            response = default;

            return false;
        }

        private static bool TryProcessError([NotNull] string error)
        {
            if (!Json.TryDeserialize(error, out ErrorResponse response))
            {
                return false;
            }

            var builder = new StringBuilder();
            builder.Append(response.Error).Append("\n");

            if (response.Data.Count > 0)
            {
                foreach (string data in response.Data)
                {
                    builder.Append($"  - {data}\n");
                }
            }

            Logger.Error(builder.ToString());

            return true;
        }

        private sealed class ErrorResponse
        {
            [JsonProperty("error")] public string Error { get; set; }
            [JsonProperty("data")] public List<string> Data { get; set; }
        }

        private sealed class SuccessResponse
        {
            [JsonProperty("success")] public string Success { get; set; }
        }
    }
}
