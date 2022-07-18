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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using JetBrains.Annotations;
using ToolkitExt.Api;
using ToolkitExt.Api.Interfaces;
using ToolkitExt.Core.Events;
using ToolkitExt.Core.Extensions;
using ToolkitExt.Core.Handlers;
using ToolkitExt.Core.Responses;

namespace ToolkitExt.Core
{
    public sealed class PollManager
    {
        private const int BufferTimer = 10;
        private static readonly RimLogger Logger = new RimLogger("PollManager");
        private readonly ConcurrentQueue<IPoll> _polls = new ConcurrentQueue<IPoll>();
        private volatile bool _concluding;
        private IPoll _current;
        private volatile bool _dequeuing;

        private PollManager()
        {
            BackendClient.Instance.RegisterHandler(new VoteHandler());
            BackendClient.Instance.RegisterHandler(new QueuedPollHandler());
        }

        public int PollDuration { get; set; } = 5;

        public static PollManager Instance { get; } = new PollManager();

        [CanBeNull]
        public IPoll CurrentPoll
        {
            get
            {
                if (_current == null && !_dequeuing)
                {
                    Task.Run(NextPollAsync);
                }

                return _current;
            }
        }

        public event EventHandler<PollStartedEventArgs> PollStarted;

        public void Queue(IPoll poll)
        {
            _polls.Enqueue(poll);
        }

        private async Task NextPollAsync()
        {
            _dequeuing = true;

            if (_current != null || !_polls.TryDequeue(out IPoll next))
            {
                _dequeuing = false;

                return;
            }

            bool shouldQueue = await next.PreQueue();

            if (!shouldQueue)
            {
                _dequeuing = false;

                return;
            }

            _current = next;
            await next.PostQueue();

            OnPollStarted(new PollStartedEventArgs { Poll = _current });

            _dequeuing = false;
        }

        public void ConcludePoll()
        {
            if (_concluding || _current == null || DateTime.UtcNow < _current?.EndedAt)
            {
                return;
            }

            _concluding = true;
            Task.Run(async () => await ConcludePollInternal());
        }

        private async Task ConcludePollInternal()
        {
            await _current.PreDelete();
            await DeletePoll();
            await _current.PostDelete();

            await CompletePollAsync();
            _concluding = false;
        }

        private async Task CompletePollAsync()
        {
            string actionName;
            Action chosenAction;

            lock (_current.Options)
            {
                IOption winner = _current.GetWinningOption();

                if (winner == null)
                {
                    Logger.Warn($@"Could not get a winning option for the poll ""{_current.Caption}"" (#{_current.Id})");

                    return;
                }

                actionName = winner.Label;
                chosenAction = winner.ChosenAction;
            }

            try
            {
                await chosenAction.OnMainAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"Encountered an error executing {actionName}", e);
            }

            _current = null;
            _concluding = false;
        }

        private async Task DeletePoll()
        {
            // We'll wait 10 seconds to ensure the backend received all the votes.
            await Task.Delay(BufferTimer * 1000);

            DeletePollResponse response = await BackendClient.Instance.DeletePoll();

            if (response == null)
            {
                Logger.Warn("Could not delete current running poll, but there's an active poll. Was it not sent?");

                return;
            }

            if (CurrentPoll == null)
            {
                Logger.Warn("A poll was deleted from the api, but there was no active poll within the manager. Discarding results...");

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
        }

        private void OnPollStarted(PollStartedEventArgs e)
        {
            PollStarted?.Invoke(this, e);
        }
    }
}
