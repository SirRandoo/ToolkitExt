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
using System.IO;
using JetBrains.Annotations;
using ToolkitExt.Api;
using ToolkitExt.Core;
using UnityEngine;
using Verse;

namespace ToolkitExt.Mod
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithMembers)]
    public class ExtensionSettings : ModSettings
    {
        internal AuthSettings Auth;
        public PollSettings Polls = new PollSettings();
        internal WindowSettings Window = new WindowSettings();

        /// <inheritdoc/>
        public override void ExposeData()
        {
            Scribe_Deep.Look(ref Window, "WindowSettings");
            Scribe_Deep.Look(ref Polls, "PollSettings");
        }

        internal void LoadAuthSettings()
        {
            if (!File.Exists(FilePaths.AuthSettings))
            {
                return;
            }

            try
            {
                Json.Load<AuthSettings>(FilePaths.AuthSettings);
            }
            catch (Exception e)
            {
                // TODO: Log that the auth settings couldn't be loaded.
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
                // TODO: Log that the auth settings couldn't be saved.
            }
        }

        /// <summary>
        ///     An internal class for housing window related settings.
        /// </summary>
        /// <remarks>
        ///     This class serves as as a means of persisting window preferences,
        ///     like the position the user last moved it to. Settings here should
        ///     not be displayed to the user, nor should they be changed outside
        ///     their relevant code.
        /// </remarks>
        internal class WindowSettings : IExposable
        {
            /// <summary>
            ///     The last known position of the poll display window.
            /// </summary>
            internal Vector2 PollPosition;

            /// <inheritdoc/>
            public void ExposeData()
            {
                Scribe_Values.Look(ref PollPosition, "PollPosition", new Vector2(Screen.width, Mathf.FloorToInt(Screen.height * 0.333f)));
            }
        }

        /// <summary>
        ///     A class for housing the poll related settings.
        /// </summary>
        public class PollSettings : IExposable
        {
            /// <summary>
            ///     Whether the poll will display bars indicating the which choices
            ///     have a certain percentage of the total number of votes casted.
            /// </summary>
            public bool Bars;
            /// <summary>
            ///     Whether the poll will use a minimal amount of color, if
            ///     applicable.
            /// </summary>
            public bool Colorless;

            /// <summary>
            ///     The number of seconds the poll will be active for.
            /// </summary>
            /// <remarks>
            ///     Despite this setting, there will always be a couple of seconds
            ///     added to the poll's duration to accommodate users with poor
            ///     connection.
            /// </remarks>
            public int Duration;

            /// <summary>
            ///     Whether the poll will use a larger text size to increase
            ///     readability.
            /// </summary>
            public bool LargeText;

            /// <inheritdoc/>
            public void ExposeData()
            {
                Scribe_Values.Look(ref Colorless, "Colorless");
                Scribe_Values.Look(ref Bars, "DisplayBars", true);
                Scribe_Values.Look(ref LargeText, "LargeText");
                Scribe_Values.Look(ref Duration, "Duration", 300);
            }
        }
    }
}
