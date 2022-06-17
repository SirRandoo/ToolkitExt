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
using JetBrains.Annotations;
using ToolkitExt.Api.Interfaces;
using ToolkitExt.Core;
using UnityEngine;
using Verse;

namespace ToolkitExt.Mod.Windows
{
    [StaticConstructorOnStartup]
    public class PollWindow : Window
    {
        internal const float Width = 310f;
        internal const float BaseHeight = 150f;
        private static readonly Gradient TimerGradient;
        private readonly Dictionary<Guid, float> _choicePercentages = new Dictionary<Guid, float>();
        private readonly IPoll _poll;
        private float _captionHeight;

        static PollWindow()
        {
            TimerGradient = new Gradient();

            var colorKey = new GradientColorKey[2];
            colorKey[0].color = Color.green;
            colorKey[0].time = 0.0f;
            colorKey[1].color = Color.red;
            colorKey[1].time = 1.0f;

            var alphaKey = new GradientAlphaKey[2];
            alphaKey[0].alpha = 1.0f;
            alphaKey[0].time = 0.0f;
            alphaKey[1].alpha = 1.0f;
            alphaKey[1].time = 1.0f;

            TimerGradient.SetKeys(colorKey, alphaKey);
        }

        public PollWindow([NotNull] IPoll poll)
        {
            _poll = poll;

            doCloseX = true;
            draggable = true;
            closeOnCancel = false;
            closeOnAccept = false;
            focusWhenOpened = false;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = false;
            preventCameraMotion = false;
        }

        /// <inheritdoc/>
        public override Vector2 InitialSize => new Vector2(Width, BaseHeight);

        /// <inheritdoc/>
        public override void DoWindowContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);

            if (!string.IsNullOrEmpty(_poll.Caption))
            {
                GameFont cache = Text.Font;
                Text.Font = ExtensionMod.Settings.Polls.LargeText ? GameFont.Medium : GameFont.Small;

                if (_captionHeight <= 0)
                {
                    _captionHeight = Text.CalcHeight(_poll.Caption, inRect.width);
                }

                var titleRect = new Rect(0f, 0f, inRect.width, _captionHeight);
                GUI.BeginGroup(titleRect);

                Widgets.Label(titleRect, _poll.Caption);
                Text.Font = cache;

                GUI.EndGroup();
            }

            Rect contentRect = new Rect(0f, _captionHeight, inRect.width, inRect.height - _captionHeight - Text.LineHeight).ContractedBy(4f);

            GUI.BeginGroup(contentRect);
            DrawChoices(contentRect.AtZero());
            GUI.EndGroup();

            var progressRect = new Rect(0f, inRect.height - Text.LineHeight, inRect.width, Text.LineHeight);
            GUI.BeginGroup(progressRect);
            DrawProgressBar(progressRect.AtZero());
            GUI.EndGroup();

            GUI.EndGroup();
        }

        private void DrawChoices(Rect region)
        {
            GameFont cache = Text.Font;
            Text.Font = ExtensionMod.Settings.Polls.LargeText ? GameFont.Medium : GameFont.Small;

            for (var i = 0; i < _poll.Options.Length; i++)
            {
                IOption option = _poll.Options[i];

                var lineRect = new Rect(0f, Text.LineHeight * i, region.width, Text.LineHeight);

                if (ExtensionMod.Settings.Polls.Bars)
                {
                    DrawRelativeWeight(lineRect, option);
                }

                Widgets.Label(lineRect, option.Label);
                TooltipHandler.TipRegion(lineRect, option.Tooltip);
            }

            Text.Font = cache;
        }

        private void DrawProgressBar(Rect region)
        {
            var progress = (float)((_poll.EndedAt - DateTime.UtcNow).TotalSeconds / (_poll.EndedAt - _poll.StartedAt).TotalSeconds);

            GUI.color = ExtensionMod.Settings.Polls.Colorless ? ColorLibrary.Teal : TimerGradient.Evaluate(1f - progress);
            Widgets.FillableBar(region, progress, Texture2D.whiteTexture, null, true);
            GUI.color = Color.white;
        }

        private void DrawRelativeWeight(Rect region, [NotNull] IOption option)
        {
            float relative = (float)option.Votes / _poll.TotalVotes;

            if (!_choicePercentages.TryGetValue(option.Id, out float lastRelative))
            {
                lastRelative = relative;
                _choicePercentages[option.Id] = relative;
            }

            var barRect = new Rect(region.x, region.y + 2f, Mathf.FloorToInt(region.width * lastRelative), region.height - 4f);

            if (ExtensionMod.Settings.Polls.Colorless)
            {
                Widgets.DrawLightHighlight(barRect);
            }
            else
            {
                GUI.color = new Color(0.2f, 0.8f, 0.85f, 0.4f);
                Widgets.DrawLightHighlight(barRect);
                GUI.color = Color.white;
            }
        }

        /// <inheritdoc/>
        protected override void SetInitialSizeAndPosition()
        {
            IPoll currentPoll = PollManager.Instance.CurrentPoll;

            if (currentPoll == null)
            {
                base.SetInitialSizeAndPosition();

                return;
            }

            GameFont lastFont = Text.Font;
            Text.Font = ExtensionMod.Settings.Polls.LargeText ? GameFont.Medium : GameFont.Small;

            Vector2 initialSize = InitialSize;
            var desiredWidth = 0f;

            foreach (IOption t in currentPoll.Options)
            {
                float width = Text.CalcSize(t.Label).x;

                if (width > desiredWidth)
                {
                    desiredWidth = width;
                }
            }

            float finalWidth = Mathf.Max(initialSize.x, desiredWidth);
            float finalHeight = initialSize.y + Text.LineHeight * currentPoll.Options.Length;

            windowRect = new Rect(
                Mathf.Clamp(ExtensionMod.Settings.Window.PollPosition.x, 0f, UI.screenWidth - finalWidth),
                Mathf.Clamp(ExtensionMod.Settings.Window.PollPosition.y, 0f, UI.screenWidth - finalHeight),
                Mathf.Max(initialSize.x, desiredWidth),
                finalHeight
            );

            Text.Font = lastFont;
        }

        /// <inheritdoc/>
        public override void WindowUpdate()
        {
            base.WindowUpdate();

            IPoll currentPoll = PollManager.Instance.CurrentPoll;

            if (currentPoll == null || DateTime.UtcNow > currentPoll.EndedAt)
            {
                Close();
            }
        }

        /// <inheritdoc/>
        public override void PreClose()
        {
            base.PreClose();

            if (Mathf.Abs(windowRect.x - ExtensionMod.Settings.Window.PollPosition.x) < 0.1f
                && Mathf.Abs(windowRect.y - ExtensionMod.Settings.Window.PollPosition.y) < 0.1f)
            {
                return;
            }

            ExtensionMod.Settings.Window.PollPosition = windowRect.position;
            ExtensionMod.Instance.WriteSettings();
        }
    }
}
