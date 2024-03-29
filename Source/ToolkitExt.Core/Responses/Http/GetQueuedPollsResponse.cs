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
using Newtonsoft.Json;
using ToolkitExt.Core.Models;

namespace ToolkitExt.Core.Responses
{
    public class GetQueuedPollsResponse
    {
        [JsonProperty("current_page")] public int CurrentPage { get; set; }
        [JsonProperty("data")] public List<RawQueuedPoll> Data { get; set; }
        [JsonProperty("first_page_url")] public Uri FirstPageUrl { get; set; }
        [JsonProperty("from")] public int From { get; set; }
        [JsonProperty("last_page")] public int LastPage { get; set; }
        [JsonProperty("last_page_url")] public Uri LastPageUrl { get; set; }
        [JsonProperty("link")] public List<QueuedPollLink> Links { get; set; }
        [JsonProperty("next_page_url")] public Uri NextPageUrl { get; set; }
        [JsonProperty("path")] public Uri Path { get; set; }
        [JsonProperty("per_page")] public int PerPage { get; set; }
        [JsonProperty("previous_page_url")] public Uri PreviousPageUrl { get; set; }
        [JsonProperty("to")] public int To { get; set; }
        [JsonProperty("total")] public int Total { get; set; }


        public class QueuedPollLink
        {
            [JsonProperty("url")] public Uri Url { get; set; }
            [JsonProperty("label")] public string Label { get; set; }
            [JsonProperty("active")] public bool Active { get; set; }
        }
    }
}
