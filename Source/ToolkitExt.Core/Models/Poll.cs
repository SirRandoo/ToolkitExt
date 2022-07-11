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
using System.Threading.Tasks;
using ToolkitExt.Api.Interfaces;
using ToolkitExt.Core.Responses;

namespace ToolkitExt.Core.Models
{
    public class Poll : IPoll
    {
        /// <inheritdoc/>
        public int Id { get; set; }

        /// <inheritdoc/>
        public string Caption { get; set; }

        /// <inheritdoc/>
        public int TotalVotes
        {
            get
            {
                var count = 0;

                for (var i = 0; i < Options.Length; i++)
                {
                    count += Options[i].Votes;
                }

                return count;
            }
        }

        /// <inheritdoc/>
        public DateTime EndedAt { get; set; }

        /// <inheritdoc/>
        public DateTime StartedAt { get; set; }

        /// <inheritdoc/>
        public IOption[] Options { get; set; }

        public int Duration => (int)Math.Ceiling((EndedAt - StartedAt).TotalMinutes);

        /// <inheritdoc/>
        public void RegisterVote(string userId, Guid choiceId)
        {
            for (var i = 0; i < Options.Length; i++)
            {
                IOption option = Options[i];

                if (option.Id == choiceId)
                {
                    option.RegisterVote(userId);
                }
            }
        }

        /// <inheritdoc/>
        public bool UnregisterVote(string userId)
        {
            var hits = 0;

            for (var i = 0; i < Options.Length; i++)
            {
                if (Options[i].UnregisterVote(userId))
                {
                    hits++;
                }
            }

            return hits > 0;
        }

        /// <inheritdoc/>
        public void ClearVotes()
        {
            for (var i = 0; i < Options.Length; i++)
            {
                Options[i].ClearVotes();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> PreQueue()
        {
            CreatePollResponse response = await BackendClient.Instance.SendPoll(this);

            if (response == null)
            {
                return false;
            }

            Id = response.Id;
            
            return true;
        }

        /// <inheritdoc/>
        public virtual async Task PostQueue()
        {
            StartedAt = DateTime.UtcNow;
            EndedAt = StartedAt.AddMinutes(PollManager.Instance.PollDuration);
        }

        /// <inheritdoc/>
        public virtual async Task PreDelete()
        {
        }

        /// <inheritdoc/>
        public virtual async Task PostDelete()
        {
        }
    }
}
