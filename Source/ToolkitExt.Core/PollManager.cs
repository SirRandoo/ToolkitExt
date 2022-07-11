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
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RimWorld;
using ToolkitExt.Api;
using ToolkitExt.Api.Enums;
using ToolkitExt.Api.Interfaces;
using ToolkitExt.Core.Events;
using ToolkitExt.Core.Extensions;
using ToolkitExt.Core.Helpers;
using ToolkitExt.Core.Models;
using ToolkitExt.Core.Registries;
using ToolkitExt.Core.Responses;
using Verse;

namespace ToolkitExt.Core
{
    public sealed class PollManager
    {
        public const int BufferTimer = 10;
        private static readonly RimLogger Logger = new RimLogger("PollManager");
        private readonly ConcurrentQueue<IPoll> _polls = new ConcurrentQueue<IPoll>();
        private IPoll _current;
        private volatile bool _deleteRequested;
        private volatile bool _deletingPoll;
        private volatile bool _dequeuing;
        private volatile bool _concluding;

        private PollManager()
        {
            BackendClient.Instance.RegisterHandler(new VoteEventHandler());
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
        }

        public void ConcludePoll()
        {
            if (_current == null || DateTime.UtcNow < _current?.EndedAt)
            {
                return;
            }

            if (_concluding)
            {
                return;
            }

            Task.Run(async () => await ConcludePollInternal());

            if (!_deleteRequested)
            {
                _deleteRequested = true;
                _deletingPoll = true;
                Task.Run(async () => await DeletePoll());
            }
            else if (!_deletingPoll)
            {
                CompletePollAsync();

                _deletingPoll = false;
                _deleteRequested = false;
            }
        }
        
        private async Task ConcludePollInternal()
        {
            await _current.PreDelete();
            await DeletePoll();
            await _current.PostDelete();

            await CompletePollAsync();
        }

        private async Task CompletePollAsync()
        {
            if (_current == null)
            {
                return;
            }

            string actionName = null;
            Action chosenAction = null;
            
            lock (_current.Options)
            {
                var highest = 0;

                foreach (IOption option in _current.Options)
                {
                    if (option.Votes > highest)
                    {
                        highest = option.Votes;
                    }
                }

                var winners = new List<IOption>();

                foreach (IOption option in _current.Options)
                {
                    if (option.Votes == highest)
                    {
                        winners.Add(option);
                    }
                }

                if (winners.TryRandomElement(out IOption result))
                {
                    actionName = result.Label;
                    chosenAction = result.ChosenAction;
                }
            }

            if (chosenAction != null)
            {
                try
                {
                    await chosenAction.OnMainAsync();
                }
                catch (Exception e)
                {
                    Logger.Error($"Encountered an error executing {actionName}", e);
                }
            }

            _current = null;
        }

        private async Task DeletePoll()
        {
            _deleteRequested = true;

            // We'll wait 10 seconds to ensure the backend received all the votes.
            await Task.Delay(BufferTimer * 1000);

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

        private void OnPollStarted(PollStartedEventArgs e)
        {
            PollStarted?.Invoke(this, e);
        }

        private sealed class VoteEventHandler : IWsMessageHandler
        {
            /// <inheritdoc/>
            public PusherEvent Event => PusherEvent.ViewerVoted;

            /// <inheritdoc/>
            public async Task<bool> Handle([NotNull] WsMessageEventArgs args)
            {
                var response = await args.AsEventAsync<ViewerVotedResponse>();

                if (response == null)
                {
                    return false;
                }

                if (Instance.CurrentPoll == null || Instance.CurrentPoll.Id != response.Data.PollId)
                {
                    return false;
                }

                Instance.CurrentPoll.RegisterVote(response.Data.VoterId, response.Data.OptionId);

                return true;
            }
        }

        private sealed class QueuedEventHandler : IWsMessageHandler
        {
            /// <inheritdoc/>
            public PusherEvent Event => PusherEvent.QueuedPollCreated;

            /// <inheritdoc/>
            public async Task<bool> Handle([NotNull] WsMessageEventArgs args)
            {
                var @event = await args.AsEventAsync<QueuedPollCreatedResponse>();

                if (@event == null)
                {
                    return false;
                }

                var options = new List<IOption>();

                foreach (QueuedPollCreatedResponse.QueuedOption option in @event.Data.Options)
                {
                    IncidentDef incident = GetIncident(option.ModId, option.DefName);

                    if (incident == null)
                    {
                        // Invalidate the poll as an option is missing
                        break;
                    }

                    IncidentParms @params = await GetIncidentParamsAsync(incident).OnMainAsync();

                    options.Add(incident.ToOption(@params));
                }

                Instance.Queue(new QueuedPoll { Caption = @event.Data.Title, Length = @event.Data.Length, Id = @event.Data.Id, Options = options.ToArray() });
                
                return true;
            }

            [CanBeNull] private static IncidentDef GetIncident(string mod, string defName) => IncidentRegistry.Get(mod, defName)?.Def;

            [ItemCanBeNull]
            private static async Task<IncidentParms> GetIncidentParamsAsync([NotNull] IncidentDef incident)
            {
                if (incident.TargetTagAllowed(IncidentTargetTagDefOf.World))
                {
                    return await GetWorldIncidentParamsAsync(incident);
                }

                //  If we don't support a modded target tag, we'll just return null.
                return incident.TargetsMap() ? await GetMapIncidentParamsAsync(incident) : null;
            }

            [ItemCanBeNull]
            private static async Task<IncidentParms> GetWorldIncidentParamsAsync([NotNull] IncidentDef incident)
            {
                IncidentParms @params = await StorytellerUtilityAsync.DefaultParamsNowAsync(incident.category, Find.World);
                bool canFireNow = await incident.Worker.CanFireNowAsync(@params);

                return !canFireNow ? null : @params;
            }

            [ItemCanBeNull]
            private static async Task<IncidentParms> GetMapIncidentParamsAsync([NotNull] IncidentDef incident)
            {
                IncidentParms @params = await StorytellerUtilityAsync.DefaultParamsNowAsync(incident.category, Find.AnyPlayerHomeMap);
                bool canFireNow = await incident.Worker.CanFireNowAsync(@params);

                return !canFireNow ? null : @params;
            }
        }
    }
}
