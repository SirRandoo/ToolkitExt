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

using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace ToolkitExt.Core
{
    /// <summary>
    ///     A class representing the authentication settings required for the
    ///     mod to communicate with the extension.
    /// </summary>
    [UsedImplicitly]
    public class AuthSettings
    {
        private string _broadcasterId;

        /// <summary>
        ///     The token for the given broadcaster.
        /// </summary>
        [JsonIgnore] public string Token { get; private set; }

        /// <summary>
        ///     The channel id of the given broadcaster.
        /// </summary>
        [JsonIgnore] public string ChannelId { get; private set; }

        /// <summary>
        ///     The broadcaster's unique key given by the extension.
        /// </summary>
        public string BroadcasterKey
        {
            get => _broadcasterId;
            set
            {
                _broadcasterId = value;
                Token = Regex.Match(_broadcasterId, "\\A\\d+.([a-zA-Z]|\\d)+").Value;
                ChannelId = Regex.Match(_broadcasterId, "\\d+$").Value;
            }
        }
    }
}
