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
using System.Threading.Tasks;
using JetBrains.Annotations;
using ToolkitExt.Api;
using ToolkitExt.Core;
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

            Task.Run(async () =>
                {
                    Logger.Info("Connecting to the ebs...");
                    await ConnectToEbsAsync();
                    
                    Logger.Info("Fetching poll settings from backend...");
                    await LoadPollSettingsAsync();
                }
            );
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
        }

        private static async Task ConnectToEbsAsync()
        {
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
            
            await BackendClient.Instance.SetCredentials(ExtensionMod.Settings.Auth.ChannelId, ExtensionMod.Settings.Auth.Token);
        }
    }
}
