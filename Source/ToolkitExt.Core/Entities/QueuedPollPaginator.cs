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

using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using ToolkitExt.Api;
using ToolkitExt.Core.Models;
using ToolkitExt.Core.Responses;

namespace ToolkitExt.Core.Entities
{
    public class QueuedPollPaginator
    {
        private static readonly RimLogger Logger = new RimLogger("QueuedPollPaginator");
        private readonly string _channelId;
        private readonly EbsHttpClient _client;
        private int _currentPage;
        private int _totalPages = 1;

        internal QueuedPollPaginator(EbsHttpClient client, string channelId)
        {
            _client = client;
            _channelId = channelId;
        }

        public bool HasNext => _currentPage < _totalPages;

        [ItemCanBeNull]
        public async Task<List<RawQueuedPoll>> GetNextPageAsync()
        {
            Logger.Debug($"Loading page #{++_currentPage:N0} from the server");
            GetQueuedPollsResponse response = await _client.GetQueuedPollsAsync(_channelId, _currentPage);

            if (response == null)
            {
                Logger.Debug("Response was null; aborting...");
                return null;
            }

            _totalPages = response.LastPage;

            if (response.CurrentPage == _currentPage)
            {
                Logger.Debug("Received the page we requested; returning...");
                Logger.Debug($"{response.Data.Count:N0} polls received.");
                return response.Data;
            }

            Logger.Warn($"Received response wasn't excepted response; received page #{response.CurrentPage:N0}, but excepted page #{_currentPage:N0}");

            _totalPages = 0;
            _currentPage = 0;

            return null;
        }
    }
}
