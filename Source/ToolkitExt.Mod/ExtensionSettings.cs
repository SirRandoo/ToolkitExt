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
using JetBrains.Annotations;
using Newtonsoft.Json;
using ToolkitExt.Api;
using ToolkitExt.Core;
using ToolkitExt.Core.Events;
using UnityEngine;
using Verse;

namespace ToolkitExt.Mod
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithMembers)]
    public class ExtensionSettings : ModSettings
    {
        private static readonly RimLogger Logger = new RimLogger("ToolkitSettings");

        [UsedImplicitly(ImplicitUseKindFlags.Assign)] internal AuthSettings Auth;
        [UsedImplicitly(ImplicitUseKindFlags.Assign)] public PollSettings Polls;

        private bool _displayingKey;

        public void Draw(Rect region)
        {
            var labelRect = new Rect(region.x, region.y, Mathf.FloorToInt(region.width * 0.8f), Text.SmallFontHeight);
            var fieldRect = new Rect(region.width - labelRect.width, region.y, region.width - labelRect.width, Text.SmallFontHeight);

            Widgets.Label(labelRect, "Broadcaster key");

            Auth.BroadcasterKey = _displayingKey
                ? Widgets.TextField(fieldRect, Auth.BroadcasterKey ?? string.Empty)
                : GUI.PasswordField(fieldRect, Auth.BroadcasterKey ?? string.Empty, '*');
        }

        internal void LoadAuthSettings()
        {
            try
            {
                Auth = Json.Load<AuthSettings>(FilePaths.AuthSettings) ?? new AuthSettings();
            }
            catch (Exception e)
            {
                Logger.Error(
                    $"Could not load authentication settings from {new Uri(GenFilePaths.SaveDataFolderPath).MakeRelativeUri(new Uri(FilePaths.AuthSettings))}. Things will not work",
                    e
                );
            }
        }

        internal void SaveAuthSettings()
        {
            try
            {
                Json.Save(FilePaths.AuthSettings, Auth);
            }
            catch (Exception e)
            {
                Logger.Error("Could not save authentication settings. This may not work properly next session.", e);
            }
        }

        internal void LoadClientPollSettings()
        {
            try
            {
                Polls = Json.Load<PollSettings>(FilePaths.PollSettings) ?? new PollSettings();
            }
            catch (Exception e)
            {
                Logger.Error(
                    $"Could not load poll settings from {new Uri(GenFilePaths.SaveDataFolderPath).MakeRelativeUri(new Uri(FilePaths.PollSettings))}. Preferences will be lost.",
                    e
                );
            }
        }

        internal void SaveClientPollSettings()
        {
            try
            {
                Json.Save(FilePaths.PollSettings, Polls);
            }
            catch (Exception e)
            {
                Logger.Error("Could not save poll settings. Poll preferences will be lost next session.", e);
            }
        }

        /// <summary>
        ///     A class for housing the poll related settings.
        /// </summary>
        public class PollSettings
        {
            private int _duration;

            /// <summary>
            ///     The number of minutes between polls.
            /// </summary>
            [JsonIgnore]
            public int Interval;

            /// <summary>
            ///     Whether the poll will use a larger text size to increase
            ///     readability.
            /// </summary>
            [JsonProperty("large_text")]
            public bool LargeText;

            /// <summary>
            ///     The number of minutes the poll will be active for.
            /// </summary>
            [JsonIgnore]
            public int Duration
            {
                get => _duration;
                set
                {
                    _duration = value;
                    PollManager.Instance.PollDuration = value;
                }
            }
        }

        internal void OnPollSettingsUpdated(object sender, [NotNull] PollSettingsUpdatedEventArgs e)
        {
            Polls.Interval = e.Interval;
            Polls.Duration = e.Duration;
        }
    }
}
