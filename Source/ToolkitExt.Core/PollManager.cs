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
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RimWorld.QuestGen;
using ToolkitExt.Api;
using ToolkitExt.Api.Interfaces;
using ToolkitExt.Core.Events;
using ToolkitExt.Core.Responses;
using UnityEngine;
using Verse;

namespace ToolkitExt.Core
{
    public class PollManager
    {
        private static readonly RimLogger Logger = new RimLogger("PollManager");
        private readonly Queue<IPoll> _polls = new Queue<IPoll>();
        private IPoll _current;
        private volatile bool _deletingPoll;
        private volatile bool _deleteRequested;

        private PollManager()
        {
            BackendClient.Instance.ViewerVoted += OnViewerVoted;
        }

        public int PollDuration { get; set; } = 300;

        public static PollManager Instance { get; } = new PollManager();

        [CanBeNull]
        public IPoll CurrentPoll
        {
            get
            {
                if (_current == null)
                {
                    NextPoll();
                }

                return _current;
            }
        }

        private void OnViewerVoted(object sender, [NotNull] ViewerVotedEventArgs e)
        {
            if (_current == null)
            {
                Logger.Warn("Received a vote, but the mod has no active poll.");

                return;
            }

            if (e.PollId != _current?.Id)
            {
                Logger.Warn("Received a vote for a poll that isn't the currently active poll.");

                return;
            }

            _current.UnregisterVote(e.VoterId);
            _current.RegisterVote(e.VoterId, e.OptionId);
        }

        public void Queue(IPoll poll)
        {
            _polls.Enqueue(poll);
        }

        public void NextPoll()
        {
            if (_current != null || !_polls.TryDequeue(out _current))
            {
                return;
            }

            _current.StartedAt = DateTime.UtcNow;
            _current.EndedAt = _current.StartedAt.AddSeconds(PollDuration);
            Task.Run(async () => await SendPollAsync()).ConfigureAwait(false);
        }

        private async Task SendPollAsync()
        {
            CreatePollResponse response = await BackendClient.Instance.SendPoll(_current);

            if (response == null)
            {
                _current = null;

                return;
            }

            _current.Id = response.Id;
        }

        public void ConcludePoll()
        {
            if (_current == null || DateTime.UtcNow < _current.EndedAt)
            {
                return;
            }

            if (!_deleteRequested)
            {
                _deleteRequested = true;
                _deletingPoll = true;
                Task.Run(async () => await DeletePoll());
            }
            else if (!_deletingPoll)
            {
                CompletePoll();

                _deletingPoll = false;
                _deleteRequested = false;
            }
        }

        private void CompletePoll()
        {
            if (CurrentPoll == null)
            {
                return;
            }

            lock (CurrentPoll.Options)
            {
                var highest = 0;

                foreach (IOption option in CurrentPoll.Options)
                {
                    if (option.Votes > highest)
                    {
                        highest = option.Votes;
                    }
                }

                var winners = new List<IOption>();
                
                foreach (IOption option in CurrentPoll.Options)
                {
                    if (option.Votes == highest)
                    {
                        winners.Add(option);
                    }
                }

                if (winners.TryRandomElement(out IOption result))
                {
                    result.ChosenAction();
                }
            }
        }

        private async Task DeletePoll()
        {
            _deleteRequested = true;
            
            // We'll wait 10 seconds to ensure the backend received all the votes.
            await Task.Delay(10000);
            
            DeletePollResponse response = await BackendClient.Instance.DeletePoll();

            if (response == null)
            {
                Logger.Warn("Could not delete current running poll, but there's an active poll. Was it not sent?");
                _deletingPoll = false;

                return;
            }

            if (CurrentPoll == null)
            {
                Logger.Warn("A poll was deleted from the api, but there was no active poll within the manager. Discarding results...");
                _deletingPoll = false;

                return;
            }

            lock (CurrentPoll.Options)
            {
                CurrentPoll.ClearVotes();

                foreach (DeletePollResponse.Vote vote in response.Votes)
                {
                    CurrentPoll.RegisterVote(vote.UserId, vote.ChoiceId);
                }
            }

            _deletingPoll = false;
        }
    }
}
