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

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using ToolkitExt.Api.Interfaces;
using ToolkitExt.Core;
using ToolkitExt.Mod.Windows;
using UnityEngine;
using Verse;

namespace ToolkitExt.Mod
{
    [UsedImplicitly]
    public class PollGameComponent : GameComponent
    {
        private int _lastMinute;
        private int _pollTracker;

        [SuppressMessage("ReSharper", "EmptyConstructor")]
        public PollGameComponent()
        {
        }

        /// <inheritdoc/>
        public override void GameComponentUpdate()
        {
            int currentMinute = GetCurrentMinute();

            if (currentMinute <= _lastMinute || currentMinute < 1)
            {
                return;
            }

            _lastMinute = currentMinute;
            _pollTracker += 1;

            if (_pollTracker < ExtensionMod.Settings.Polls.Interval)
            {
                return;
            }

            // TODO: Create a poll and queue it.
        }

        /// <inheritdoc/>
        public override void GameComponentOnGUI()
        {
            IPoll currentPoll = PollManager.Instance.CurrentPoll;

            if (currentPoll == null)
            {
                return;
            }

            if (!Find.WindowStack.IsOpen(typeof(PollWindow)))
            {
                Find.WindowStack.Add(new PollWindow(currentPoll));
            }
        }

        private static int GetCurrentMinute() => Mathf.FloorToInt(Time.unscaledTime / 60.0f);
    }
}
