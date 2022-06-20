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
using Newtonsoft.Json;
using ToolkitExt.Api.Converters;

namespace ToolkitExt.Core.Responses
{
    public class ViewerVotedResponse : PusherResponse
    {
        [JsonProperty("data")]
        [JsonConverter(typeof(EmbeddedJsonConverter<VoteData>))]
        public VoteData Data { get; set; }

        [JsonProperty("channel")] public string Channel { get; set; }

        public class VoteData
        {
            [JsonProperty("poll_id")] public int PollId { get; set; }
            [JsonProperty("value")] public Guid OptionId { get; set; }
            [JsonProperty("provider_id")] public string VoterId { get; set; }
        }
    }
}