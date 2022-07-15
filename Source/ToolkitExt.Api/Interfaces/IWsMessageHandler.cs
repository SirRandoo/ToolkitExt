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

using System.Threading.Tasks;
using ToolkitExt.Api.Events;

namespace ToolkitExt.Api.Interfaces
{
    /// <summary>
    ///     An interface for outlining classes that handle messages from the
    ///     EBS websocket.
    /// </summary>
    public interface IWsMessageHandler
    {
        /// <summary>
        ///     The priority of the handler.
        /// </summary>
        /// <remarks>
        ///     Handlers with a higher priority will execute before handlers with
        ///     a lower priority.
        /// </remarks>
        int Priority { get; }

        /// <summary>
        ///     Invoked when a new message is received from the websocket.
        /// </summary>
        /// <param name="args">
        ///     A <see cref="WsMessageEventArgs"/> containing the
        ///     message from the websocket
        /// </param>
        /// <returns>
        ///     Whether the websocket client should continue calling message
        ///     handlers. If <c>true</c>, the client will stop calling message
        ///     handlers. If <c>false</c>, the client will continue calling
        ///     message handlers.
        /// </returns>
        Task<bool> Handle(WsMessageEventArgs args);
    }
}
