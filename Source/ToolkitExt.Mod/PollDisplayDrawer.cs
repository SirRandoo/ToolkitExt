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
using ToolkitExt.Api.Interfaces;
using ToolkitExt.Core;
using UnityEngine;
using Verse;

namespace ToolkitExt.Mod
{
    public class PollDisplayDrawer
    {
        private const float Width = 512f;
        private const float Height = 72f;
        private float _captionHeight;
        private int _lastId = -1;
        private Rect _leftInnerRegion = Rect.zero;
        private Rect _leftRegion = Rect.zero;
        private Rect _middleInnerRect = Rect.zero;
        private Rect _middleRegion = Rect.zero;
        private Rect _captionRegion = Rect.zero;
        private Rect _region = Rect.zero;
        private Rect _regionInner = Rect.zero;
        private Rect _regionOuter = Rect.zero;
        private Rect _rightInnerRegion = Rect.zero;
        private Rect _rightRegion = Rect.zero;

        public void Invalidate()
        {
            _region = new Rect(Mathf.FloorToInt(UI.screenWidth * 0.5f) - Mathf.FloorToInt(Width * 0.5f), Mathf.FloorToInt(UI.screenHeight * 0.2f), Width, Height);

            IPoll currentPoll = PollManager.Instance.CurrentPoll;

            if (currentPoll == null || currentPoll.Options.Length < 2)
            {
                return;
            }

            if (currentPoll.Id != _lastId)
            {
                _lastId = currentPoll.Id;

                GameFont font = Text.Font;

                Text.Font = GameFont.Small;
                _captionHeight = Text.CalcSize(currentPoll.Caption).y;
                Text.Font = font;
            }

            (int index, float percentage) = GetLeadingOption(currentPoll);

            CalculateRegion();

            if (index == -1)
            {
                CalculateDefault();
            }
            else
            {
                CalculateLeading(index, percentage);
            }
            
            CalculateInnerRegions();
        }

        private void CalculateRegion()
        {
            float center = Mathf.FloorToInt(UI.screenWidth * 0.5f);
            float width = Mathf.Max(Mathf.FloorToInt(UI.screenWidth * 0.33f), 500f);
            float height = Mathf.FloorToInt(16f * Prefs.UIScale);
            float y = Mathf.FloorToInt(UI.screenHeight - 40f - height - _captionHeight);

            _region = new Rect(center - Mathf.FloorToInt(width * 0.5f), y, width, height);
            _regionInner = _region.ContractedBy(6f);
            _regionOuter = _regionInner.ExpandedBy(1f);
            _captionRegion = new Rect(_region.x, _region.y - _captionHeight - 2f, _region.width, _captionHeight);
        }

        private void CalculateDefault()
        {
            _leftRegion = new Rect(_regionInner.x, _regionInner.y, Mathf.FloorToInt(_regionInner.width * 0.5f), _regionInner.height);
            _middleRegion = new Rect(_region.center.x - Mathf.FloorToInt(_region.height * 0.5f), _region.y, _region.height, _region.height);
            _rightRegion = new Rect(_leftRegion.x + _leftRegion.width, _leftRegion.y, _leftRegion.width, _leftRegion.height);
        }

        private void CalculateLeading(int index, float percentage)
        {
            switch (index)
            {
                case 0:
                    _leftRegion = new Rect(_regionInner.x, _regionInner.y, _regionInner.width - Mathf.FloorToInt(_regionInner.width * percentage), _regionInner.height);

                    break;
                case 1:
                    _leftRegion = new Rect(_regionInner.x, _regionInner.y, Mathf.FloorToInt(_regionInner.width * percentage), _regionInner.height);

                    break;
            }

            _middleRegion = new Rect(_leftRegion.x + _leftRegion.width - _region.height, _region.y + 4f, _region.height, _region.height - 8f);
            _rightRegion = new Rect(_leftRegion.x + _leftRegion.width, _regionInner.y + 4f, _regionInner.width - _leftRegion.width, _regionInner.height - 8f);
        }

        private void CalculateInnerRegions()
        {
            _middleInnerRect = _middleRegion.ContractedBy(3f);
            _leftInnerRegion = _leftRegion.ContractedBy(2f);
            _rightInnerRegion = _rightRegion.ContractedBy(2f);
        }

        private static (int, float) GetLeadingOption([NotNull] IPoll poll)
        {
            IOption left = poll.Options[0];
            IOption right = poll.Options[1];

            if (left.Votes > right.Votes)
            {
                return (0, Mathf.Max(left.Votes / (float)poll.TotalVotes));
            }

            return right.Votes > left.Votes ? (1, Mathf.Max(right.Votes / (float)poll.TotalVotes)) : (-1, 0);
        }

        public void Draw()
        {
            IPoll currentPoll = PollManager.Instance.CurrentPoll;

            if (currentPoll == null || currentPoll.Options.Length < 2)
            {
                return;
            }

            Widgets.DrawAtlas(_regionOuter, Textures.WindowAtlas);

            DrawLeftOption(currentPoll.Options[0]);
            DrawRightOption(currentPoll.Options[1]);
            DrawEmblem();
            DrawTimer();

            if (string.IsNullOrEmpty(currentPoll.Caption))
            {
                return;
            }

            TextAnchor anchor = Text.Anchor;

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(_captionRegion, currentPoll.Caption);
            Text.Anchor = anchor;
        }

        private void DrawLeftOption([NotNull] IOption option)
        {
            var leftTextRect = new Rect(_leftRegion.x, _leftRegion.y + _leftRegion.height + 2f, _leftRegion.width, _captionHeight);

            Color old = GUI.color;
            GUI.color = ColorLibrary.Rose;
            GUI.DrawTexture(_leftInnerRegion, Textures.ProgressLeftAtlas);
            GUI.color = old;

            TextAnchor cache = Text.Anchor;

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(leftTextRect, option.Label);

            Text.Anchor = cache;
        }

        private void DrawRightOption([NotNull] IOption option)
        {
            var rightTextRect = new Rect(_rightRegion.x, _rightRegion.y + _rightRegion.height + 2f, _rightRegion.width, _captionHeight);

            Color old = GUI.color;
            GUI.color = ColorLibrary.Magenta;
            GUI.DrawTexture(_rightInnerRegion, Textures.ProgressRightAtlas);
            GUI.color = old;

            TextAnchor cache = Text.Anchor;

            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(rightTextRect, option.Label);

            Text.Anchor = cache;
        }

        private void DrawEmblem()
        {
            GUI.DrawTexture(_middleRegion, Widgets.ButtonBGAtlas);
            GUI.DrawTexture(_middleInnerRect, Textures.Toolbox);
        }

        private void DrawTimer()
        {
            IPoll currentPoll = PollManager.Instance.CurrentPoll!;

            double seconds = (currentPoll.EndedAt - DateTime.UtcNow).TotalSeconds + PollManager.BufferTimer;

            var timerRect = new Rect(
                _middleRegion.x - Mathf.FloorToInt(_middleRegion.width * 0.5f),
                _middleRegion.y + _middleRegion.height,
                _middleRegion.width * 2f,
                Mathf.FloorToInt(_captionHeight * 1.5f)
            );

            GameFont font = Text.Font;
            TextAnchor anchor = Text.Anchor;

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(timerRect, seconds.ToString("N0"));
            Text.Anchor = anchor;
            Text.Font = font;
        }
    }
}
