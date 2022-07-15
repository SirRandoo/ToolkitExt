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

using System.Threading.Tasks;
using JetBrains.Annotations;
using ToolkitExt.Api.Enums;
using ToolkitExt.Api.Events;
using ToolkitExt.Api.Interfaces;

namespace ToolkitExt.Core
{
    /// <summary>
    ///     A class for filtering websocket messages by a given
    ///     <see cref="PusherEvent"/>.
    /// </summary>
    public abstract class FilteredMessageHandler : IWsMessageHandler
    {
        private readonly PusherEvent _event;

        protected FilteredMessageHandler(PusherEvent @event) : this(@event, 1)
        {
        }

        protected FilteredMessageHandler(PusherEvent @event, int priority)
        {
            _event = @event;
            Priority = priority;
        }

        /// <inheritdoc/>
        public int Priority { get; }

        /// <inheritdoc/>
        public async Task<bool> Handle([NotNull] WsMessageEventArgs args)
        {
            if (_event == args.EventId)
            {
                return await HandleEvent(args);
            }

            return false;
        }

        /// <summary>
        ///     Invoked when a websocket message is received that matches the
        ///     filter specified.
        /// </summary>
        /// <param name="args">
        ///     A <see cref="WsMessageEventArgs"/> containing
        ///     information about the websocket message
        /// </param>
        /// <returns>
        ///     Whether the websocket client should continue calling message
        ///     handlers. If <c>true</c>, the client will stop calling message
        ///     handlers. If <c>false</c>, the client will continue calling
        ///     message handlers.
        /// </returns>
        protected abstract Task<bool> HandleEvent(WsMessageEventArgs args);
    }
}
