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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using JetBrains.Annotations;
using ToolkitExt.Api.Enums;
using ToolkitExt.Api.Interfaces;
using ToolkitExt.Api.Registries;
using ToolkitExt.Core;
using ToolkitExt.Core.Responses;
using ToolkitExt.Mod.UX;
using UnityEngine;
using Verse;

namespace ToolkitExt.Mod
{
    [UsedImplicitly]
    public class PollGameComponent : GameComponent
    {
        private readonly PollDisplayDrawer _drawer = new PollDisplayDrawer();
        private float _lastHeight = UI.screenHeight;
        private int _lastMinute;
        private float _lastScale = Prefs.UIScale;
        private float _lastWidth = UI.screenWidth;
        private bool _pollStarted;
        private int _pollTracker;

        public PollGameComponent(Game game)
        {
            PollManager.Instance.PollStarted += (_, __) => _pollStarted = true;
        }

        /// <inheritdoc/>
        public override void GameComponentTick()
        {
            if (PollManager.Instance.CurrentPoll == null || DateTime.UtcNow < PollManager.Instance.CurrentPoll.EndedAt)
            {
                return;
            }

            PollManager.Instance.ConcludePoll();
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

            _pollTracker = 0;

            foreach (IPollFactory factory in PollFactoryRegistry.AllFactoriesRandom)
            {
                IPoll poll = factory.Create();

                if (poll == null)
                {
                    continue;
                }

                PollManager.Instance.Queue(poll);

                break;
            }
        }

        /// <inheritdoc/>
        public override void GameComponentOnGUI()
        {
            IPoll currentPoll = PollManager.Instance.CurrentPoll;

            if (currentPoll == null || DateTime.UtcNow > currentPoll.EndedAt)
            {
                return;
            }

            if (_pollStarted || _drawer.IsTransitioning)
            {
                _drawer.Invalidate();
                _pollStarted = false;
            }

            ValidateGameSettings();
            _drawer.Draw();
        }

        private void ValidateGameSettings()
        {
            var shouldRecalculate = false;

            if (Mathf.Abs(_lastHeight - UI.screenHeight) > 0.01f)
            {
                _lastHeight = UI.screenHeight;
                shouldRecalculate = true;
            }

            if (Mathf.Abs(_lastWidth - UI.screenWidth) > 0.01f)
            {
                _lastWidth = UI.screenWidth;
                shouldRecalculate = true;
            }

            if (Mathf.Abs(_lastScale - Prefs.UIScale) > 0.01f)
            {
                _lastScale = Prefs.UIScale;
                shouldRecalculate = true;
            }

            if (shouldRecalculate)
            {
                _drawer.Invalidate();
            }
        }

        private static int GetCurrentMinute() => Mathf.FloorToInt(Time.unscaledTime / 60.0f);
    }
}
