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
using Steamworks;
using ToolkitExt.Api;
using ToolkitExt.Core;
using ToolkitExt.Core.Entities;
using ToolkitExt.Core.Models;
using ToolkitExt.Core.Registries;
using ToolkitExt.Core.Responses;
using Verse;

namespace ToolkitExt.Mod
{
    [UsedImplicitly]
    [StaticConstructorOnStartup]
    internal static class ExtensionRunner
    {
        private static readonly RimLogger Logger = new RimLogger("ToolkitExt");

        static ExtensionRunner()
        {
            Logger.Info("Performing startup operations...");

            Logger.Info("Loading authentication settings...");
            ExtensionMod.Settings.LoadAuthSettings();

            Logger.Info("Loading client poll settings...");
            ExtensionMod.Settings.LoadClientPollSettings();

            Logger.Info("Registering settings handler...");
            BackendClient.Instance.RegisterHandler(new ExtensionSettings.PollSettingsHandler());

            if (ExtensionMod.Settings.Auth == null)
            {
                Logger.Warn("Authentication settings was null; connection aborted.");

                return;
            }

            if (string.IsNullOrEmpty(ExtensionMod.Settings.Auth.ChannelId))
            {
                Logger.Warn("Channel id is invalid; reenter a broadcaster key, or make sure you copied it right.");

                return;
            }

            if (string.IsNullOrEmpty(ExtensionMod.Settings.Auth.Token))
            {
                Logger.Warn("Channel token is invalid; reenter a broadcaster key, or make sure you copied it right.");

                return;
            }

            Task.Run(async () => await DoStartupOperationsAsync());
        }
        private static async Task DoStartupOperationsAsync()
        {
            Logger.Info("Connecting to the ebs...");
            await ConnectToEbsAsync();

            Logger.Info("Fetching poll settings from backend...");
            await LoadPollSettingsAsync();

            Logger.Info("Syncing incidents...");
            IncidentRegistry.Sync();

            Logger.Info("Loading queued polls...");
            await LoadQueuedPollsAsync();
            Logger.Info("Finished loading queued polls!");
        }
        
        private static async Task LoadQueuedPollsAsync()
        {
            Logger.Debug("Grabbing queued poll paginator...");
            QueuedPollPaginator paginator = await BackendClient.Instance.GetQueuedPolls();

            Logger.Debug("Getting initial page from the server...");
            List<RawQueuedPoll> queue = await paginator.GetNextPageAsync();

            if (queue == null)
            {
                Logger.Debug("There was no queued polls from the server; aborting...");

                return;
            }
            
            Logger.Debug($"Received {queue.Count:N0} polls from the server (initial).");

            while (paginator.HasNext)
            {
                List<RawQueuedPoll> polls = await paginator.GetNextPageAsync();

                if (polls == null)
                {
                    Logger.Debug("Reached end of list; aborting...");
                    
                    return;
                }

                Logger.Debug($"Received {polls.Count:N0} polls from the server.");
                
                queue.AddRange(polls);
            }

            Logger.Debug("Registering polls to queued poll repository...");
            foreach (RawQueuedPoll poll in queue)
            {
                QueuedPollRepository.Add(poll);
            }
        }

        private static async Task LoadPollSettingsAsync()
        {
            PollSettingsResponse response = await BackendClient.Instance.GetPollSettings();

            if (response == null)
            {
                Logger.Warn("Poll settings was null");

                return;
            }

            ExtensionMod.Settings.Polls.Duration = response.Duration;
            ExtensionMod.Settings.Polls.Interval = response.Interval;
            ExtensionMod.Settings.Polls.AutomatedPolls = response.AutomatedPolls;
        }

        private static async Task ConnectToEbsAsync()
        {
            await BackendClient.Instance.SetCredentials(ExtensionMod.Settings.Auth.ChannelId, ExtensionMod.Settings.Auth.Token);
        }
    }
}
