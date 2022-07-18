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
using ToolkitExt.Api;
using ToolkitExt.Api.Enums;
using ToolkitExt.Api.Events;
using ToolkitExt.Core.Responses;
using UnityEngine;

namespace ToolkitExt.Core.Handlers
{
    internal sealed class VoteHandler : FilteredMessageHandler
    {
        private static readonly RimLogger Logger = new RimLogger("VoteHandler");
    
        internal VoteHandler() : base(PusherEvent.ViewerVoted)
        {
        }

        /// <inheritdoc/>
        protected override async Task<bool> HandleEvent([NotNull] WsMessageEventArgs args)
        {
            var response = await args.AsEventAsync<ViewerVotedResponse>();

            if (response == null)
            {
                Logger.Debug("Message received was not a vote response; ignoring.");
                return false;
            }

            if (PollManager.Instance.CurrentPoll == null)
            {
                Logger.Debug($"Received a vote for poll #{response.Data.PollId}, but there is no active poll in the manager.");
                return false;
            }

            if (PollManager.Instance.CurrentPoll.Id != response.Data.PollId)
            {
                Logger.Debug(
                    $"Received a vote for poll #{response.Data.PollId}, but it does not match the current poll of #{PollManager.Instance.CurrentPoll.Id}; ignoring.."
                );
                
                return false;
            }

            Logger.Debug("Registering vote...");
            PollManager.Instance.CurrentPoll.RegisterVote(response.Data.VoterId, response.Data.OptionId);

            return true;
        }
    }
}
