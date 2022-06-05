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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace ToolkitExt.Core.Requests
{
    public class PollRequest
    {
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("length")] public int Length { get; set; }
        [JsonProperty("options")] public List<Option> Options { get; } = new List<Option>();

        public bool AddOption(string id, string label)
        {
            for (var i = 0; i < Options.Count; i++)
            {
                if (string.Equals(Options[i].Id, id))
                {
                    return false;
                }
            }

            Options.Add(new Option { Id = id, Label = label });

            return true;
        }

        public bool RemoveOption(string id)
        {
            for (int i = Options.Count - 1; i >= 0; i--)
            {
                if (string.Equals(Options[i].Id, id))
                {
                    Options.RemoveAt(i);

                    return true;
                }
            }

            return false;
        }

        public class Option
        {
            [JsonProperty("value")] public string Id { get; set; }
            [JsonProperty("label")] public string Label { get; set; }
        }
    }
}
