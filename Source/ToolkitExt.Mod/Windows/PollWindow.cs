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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using SirRandoo.CommonLib.Helpers;
using ToolkitExt.Api.Interfaces;
using ToolkitExt.Core;
using ToolkitExt.Core.Events;
using UnityEngine;
using Verse;

namespace ToolkitExt.Mod.Windows
{
    internal class PollWindow : Window
    {
        private const float MinWidth = 512f;
        private const float MinHeight = 72f;
        private const float BarSpanMultiplier = 1.5f;
        private const string RawLeftBarColor = "#F7941E";
        private const string RawRightBarColor = "#1E81F7";

        private static readonly Color LeftBarColor;
        private static readonly Color RightBarColor;
        private Rect _barEmblemRegion = Rect.zero;

        private Rect _barRegion = Rect.zero;

        private Rect _captionRegion = Rect.zero;
        private GameFont _font = GameFont.Small;
        private Rect _innerBarEmblemRegion = Rect.zero;
        private Rect _innerBarRegion = Rect.zero;

        private int _lastId = -1;
        private float _lastPercentage = 1f;

        private int _lastSecond;
        private Rect _leftBarRegion = Rect.zero;
        private Rect _leftLabelRegion = Rect.zero;

        private Rect _metadataRegion = Rect.zero;
        private bool _recalculating = true;
        private Rect _rightBarRegion = Rect.zero;
        private Rect _rightLabelRegion = Rect.zero;
        private string _timer = "0";
        private Rect _timerRegion = Rect.zero;
        private bool _transitioning;
        private bool _updateWindowSize;

        static PollWindow()
        {
            if (!ColorUtility.TryParseHtmlString(RawLeftBarColor, out LeftBarColor))
            {
                LeftBarColor = ColorLibrary.Orange;
            }

            if (ColorUtility.TryParseHtmlString(RawRightBarColor, out RightBarColor))
            {
                RightBarColor = ColorLibrary.Blue;
            }
        }

        public PollWindow()
        {
            doCloseX = false;
            draggable = true;
            soundClose = null;
            soundAppear = null;
            drawShadow = false;
            closeOnCancel = false;
            closeOnAccept = false;
            focusWhenOpened = false;
            doWindowBackground = false;
            preventCameraMotion = false;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = false;

            layer = WindowLayer.GameUI;

            PollManager.Instance.ViewerVoted += OnViewerVoted;
        }

        /// <inheritdoc/>
        protected override float Margin => 0f;

        /// <inheritdoc/>
        public override Vector2 InitialSize
        {
            get
            {
                GameFont oldFont = Text.Font;
                Text.Font = _font;

                IPoll currentPoll = PollManager.Instance.CurrentPoll;

                if (currentPoll == null)
                {
                    return new Vector2(MinWidth, MinHeight);
                }

                float maxTimerWidth = Text.CalcSize("300").x;
                float captionWidth = Text.CalcSize(currentPoll.Caption ?? "").x;
                float leftLabelWidth = Text.CalcSize(currentPoll.Options[0].Label).x;
                float rightLabelWidth = Text.CalcSize(currentPoll.Options[1].Label).x;
                float metadataWidth = maxTimerWidth + leftLabelWidth + rightLabelWidth;
                float highestWidth = Mathf.Max(captionWidth, metadataWidth, MinWidth);
                float lineHeight = Text.LineHeight;
                float highestHeight = Mathf.Max(lineHeight * (2f + BarSpanMultiplier), MinHeight);

                Text.Font = oldFont;

                return new Vector2(highestWidth, highestHeight);
            }
        }

        /// <inheritdoc/>
        public override void DoWindowContents(Rect inRect)
        {
            Color oldColor = GUI.color;

            GUI.BeginGroup(inRect);

            IPoll poll = PollManager.Instance.CurrentPoll;

            if (poll == null || poll.Options.Length <= 0)
            {
                return;
            }

            IOption first = poll.Options[0];
            IOption second = poll.Options[1];

            UiHelper.Label(_timerRegion, _timer, TextAnchor.MiddleCenter, _font);
            UiHelper.Label(_leftLabelRegion, first.Label, TextAnchor.MiddleLeft, _font);
            UiHelper.Label(_captionRegion, poll.Caption, TextAnchor.MiddleCenter, _font);
            UiHelper.Label(_rightLabelRegion, second.Label, TextAnchor.MiddleRight, _font);

            Widgets.DrawAtlas(_barRegion, Textures.WindowAtlas);

            GUI.color = LeftBarColor;
            Widgets.DrawAtlas(_leftBarRegion, Textures.ProgressLeftAtlas);

            GUI.color = RightBarColor;
            Widgets.DrawAtlas(_rightBarRegion, Textures.ProgressRightAtlas);

            GUI.color = oldColor;
            Widgets.DrawAtlas(_barEmblemRegion, Widgets.ButtonBGAtlas);

            UiHelper.Icon(_innerBarEmblemRegion, Textures.Toolbox, null);

            GUI.EndGroup();
        }

        private void Recalculate()
        {
            _recalculating = false;

            Text.Font = ExtensionMod.Settings.Polls.LargeText ? GameFont.Medium : GameFont.Small;

            if (_updateWindowSize)
            {
                Vector2 initial = InitialSize;
                var newWindowRegion = new Rect(windowRect.x - Mathf.FloorToInt(initial.x * 0.5f), windowRect.y - Mathf.FloorToInt(initial.y * 0.5f), initial.x, initial.y);

                windowRect = newWindowRegion;

                _updateWindowSize = false;
            }

            float lineHeight = Text.LineHeight;
            float timerWidth = Text.CalcSize("300").x;
            float halfTimerWidth = Mathf.FloorToInt(timerWidth * 0.5f);

            _captionRegion = new Rect(0f, 0f, windowRect.width, lineHeight);

            _barRegion = new Rect(0f, lineHeight, windowRect.width, Mathf.FloorToInt(lineHeight * BarSpanMultiplier));
            _innerBarRegion = _barRegion.ContractedBy(5f);

            _metadataRegion = new Rect(0f, windowRect.height - lineHeight, windowRect.width, lineHeight);
            _timerRegion = new Rect(Mathf.FloorToInt(_metadataRegion.width * 0.5f) - halfTimerWidth, _metadataRegion.y, timerWidth, lineHeight);
            _leftLabelRegion = new Rect(0f, _metadataRegion.y, Mathf.FloorToInt(_metadataRegion.width * 0.5f) - halfTimerWidth, _metadataRegion.height);
            _rightLabelRegion = new Rect(_timerRegion.x + _timerRegion.width, _metadataRegion.y, _leftLabelRegion.width, _metadataRegion.height);

            CalculateLeading();
        }

        private void CalculateLeading()
        {
            (int index, float percentage) = GetLeadingOption();

            if (Mathf.Abs(_lastPercentage - percentage) > 0.01f)
            {
                _lastPercentage = Mathf.SmoothStep(_lastPercentage, percentage, 0.05f);
                _transitioning = true;
            }
            else
            {
                _transitioning = false;
            }

            switch (index)
            {
                case 0:
                    _leftBarRegion = new Rect(
                        _innerBarRegion.x,
                        _innerBarRegion.y,
                        _innerBarRegion.width - Mathf.FloorToInt(_innerBarRegion.width * _lastPercentage),
                        _innerBarRegion.height
                    );

                    break;
                case 1:
                    _leftBarRegion = new Rect(
                        _innerBarRegion.x,
                        _innerBarRegion.y,
                        Mathf.FloorToInt(_innerBarRegion.width * Mathf.Clamp(_lastPercentage, 0, 1f)),
                        _innerBarRegion.height
                    );

                    break;
                default:
                    _leftBarRegion = new Rect(_innerBarRegion.x, _innerBarRegion.y, Mathf.FloorToInt(_innerBarRegion.width * 0.5f), _innerBarRegion.height);

                    break;
            }

            _barEmblemRegion = new Rect(
                _leftBarRegion.width <= 1f ? _leftBarRegion.x : _leftBarRegion.x + _leftBarRegion.width - Mathf.FloorToInt(_barRegion.height * 0.5f),
                _barRegion.y,
                _barRegion.height,
                _barRegion.height
            );

            _innerBarEmblemRegion = _barEmblemRegion.ContractedBy(2f);

            _rightBarRegion = new Rect(_leftBarRegion.x + _leftBarRegion.width, _innerBarRegion.y, _innerBarRegion.width - _leftBarRegion.width, _innerBarRegion.height);
        }

        private static (int, float) GetLeadingOption()
        {
            IPoll poll = PollManager.Instance.CurrentPoll;

            IOption left = poll!.Options[0];
            IOption right = poll.Options[1];

            // The display is scaled from 0 at the left end, and 200 on the right end.
            // With that noted, this method will always return the adjusted percentage
            // of the winning option. This is to ensure the emblem will always slide to
            // the correct position on the display.
            if (left.Votes > right.Votes)
            {
                return (0, 1f - left.Votes / (float)poll.TotalVotes);
            }

            if (right.Votes > left.Votes)
            {
                return (1, 2f - right.Votes / (float)poll.TotalVotes);
            }

            return (-1, 0.5f);
        }

        private void OnViewerVoted(object sender, ViewerVotedEventArgs e)
        {
            _recalculating = true;
        }

        /// <inheritdoc/>
        public override void WindowUpdate()
        {
            base.WindowUpdate();

            IPoll currentPoll = PollManager.Instance.CurrentPoll;

            if (currentPoll == null)
            {
                return;
            }

            int second = Mathf.FloorToInt(Time.unscaledTime);

            if (second > _lastSecond)
            {
                _lastSecond = second;

                GameFont newFont = ExtensionMod.Settings.Polls.LargeText ? GameFont.Medium : GameFont.Small;

                if (newFont != _font)
                {
                    _font = newFont;
                    _recalculating = true;
                    _updateWindowSize = true;
                }

                double seconds = (currentPoll.EndedAt - DateTime.UtcNow).TotalSeconds;
                _timer = seconds.ToString("N0");

                if (seconds <= 0)
                {
                    Close(false);

                    return;
                }
            }

            if (currentPoll.Id != _lastId)
            {
                _recalculating = true;
                _lastId = currentPoll.Id;
            }

            if (_transitioning || _recalculating)
            {
                Recalculate();
            }
        }

        /// <inheritdoc/>
        protected override void SetInitialSizeAndPosition()
        {
            Vector2 initialSize = InitialSize;
            float x = Mathf.FloorToInt(UI.screenWidth * 0.5f) - Mathf.FloorToInt(initialSize.x * 0.5f);
            float y = Mathf.FloorToInt(UI.screenHeight - 45f - initialSize.y);

            if (Find.MainTabsRoot.OpenTab != null)
            {
                y -= Find.MainTabsRoot.OpenTab.TabWindow.InitialSize.y;
            }

            windowRect = new Rect(Mathf.Clamp(x, 0, UI.screenWidth), Mathf.Clamp(y, 0, UI.screenHeight), initialSize.x, initialSize.y);
        }

        /// <inheritdoc/>
        public override void PostOpen()
        {
            base.PostOpen();

            ExtensionMod.Settings.Windows.PollX = Mathf.FloorToInt(windowRect.x);
            ExtensionMod.Settings.Windows.PollY = Mathf.FloorToInt(windowRect.y);
            ExtensionMod.Settings.SaveClientWindowSettings();
        }
    }
}
