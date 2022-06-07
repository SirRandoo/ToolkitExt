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
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using RestSharp;
using ToolkitExt.Api;
using ToolkitExt.Core.Requests;
using ToolkitExt.Core.Responses;

namespace ToolkitExt.Core
{
    public class EbsHttpClient
    {
        private static readonly Uri APIUrl = new Uri("https://tkx-toolkit.jumpingcrab.com/api/");
        private readonly RestClient _client = new RestClient(APIUrl);
        private string _token;

        [NotNull] public static EbsHttpClient Instance { get; } = new EbsHttpClient();

        public void SetToken(string token)
        {
            _token = token;
        }

        [NotNull]
        private RestRequest GetRequest([NotNull] string endpoint, Method method)
        {
            var request = new RestRequest(endpoint, method, DataFormat.Json);
            request.AddHeader("Authorization", $"Bearer {_token}");

            return request;
        }

        [ItemCanBeNull]
        public async Task<AuthResponse> RetrieveToken(string socketId, string channel)
        {
            RestRequest request = GetRequest("/broadcasting/auth", Method.POST);
            request.AddJsonBody(new BroadcastTokenRequest { ChannelName = channel, SocketId = socketId });

            IRestResponse response = await _client.ExecuteAsync(request);

            try
            {
                return Json.Deserialize<AuthResponse>(response.Content);
            }
            catch (JsonException)
            {
                // TODO: Log that the token couldn't be retrieved.
                return null;
            }
        }

        [ItemCanBeNull]
        public async Task<CreatePollResponse> CreatePollAsync([NotNull] PollRequest poll)
        {
            RestRequest request = GetRequest("/broadcasting/polls/create", Method.POST);
            request.AddJsonBody(poll);
            
            IRestResponse response = await _client.ExecuteAsync(request);

            try
            {
                return Json.Deserialize<CreatePollResponse>(response.Content);
            }
            catch (JsonException)
            {
                // TODO: Log that a creation request was rejected.
                return null;
            }
        }

        [ItemCanBeNull]
        public async Task<DeletePollResponse> DeletePollAsync()
        {
            RestRequest request = GetRequest("/broadcasting/polls/delete", Method.DELETE);
            IRestResponse<DeletePollResponse> response = await _client.ExecuteAsync<DeletePollResponse>(request);

            return !response.IsSuccessful ? null : response.Data;
        }
    }
}
