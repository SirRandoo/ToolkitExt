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

using JetBrains.Annotations;
using ToolkitExt.Api.Interfaces;
using ToolkitExt.Core;
using UnityEngine;
using Verse;

namespace ToolkitExt.Mod
{
    public class PollDisplayDrawer
    {
        private Rect _region = Rect.zero;
        private Rect _leftRegion = Rect.zero;
        private Rect _leftInnerRegion = Rect.zero;
        private Rect _rightRegion = Rect.zero;
        private Rect _rightInnerRegion = Rect.zero;
        private Rect _middleRegion = Rect.zero;
        private Rect _middleInnerRect = Rect.zero;

        public void Invalidate()
        {
            var y = 0f;

            foreach (Vector2 loc in Find.ColonistBar.DrawLocs)
            {
                if (loc.y > y)
                {
                    y = loc.y;
                }
            }

            float width = Mathf.FloorToInt(UI.screenWidth * 0.25f);
            float x = Mathf.FloorToInt(UI.screenWidth * 0.5f) - Mathf.FloorToInt(width * 0.5f);
            float height = Mathf.FloorToInt(Find.ColonistBar.Size.y * 0.25f);

            _region = new Rect(x, y + Find.ColonistBar.Size.y + 5f, width, height);

            IPoll currentPoll = PollManager.Instance.CurrentPoll;

            if (currentPoll == null || currentPoll.Options.Length < 2)
            {
                return;
            }

            IOption left = currentPoll.Options[0];
            
            int totalVotes = currentPoll.TotalVotes;
            float leftWidth = Mathf.FloorToInt(_region.width * Mathf.FloorToInt(left.Votes / (float) totalVotes));
            float rightWidth = _region.width - leftWidth;

            _leftRegion = new Rect(_region.x, _region.y, leftWidth, _region.height);
            _leftInnerRegion = _leftRegion.ContractedBy(1f);
            
            _rightRegion = new Rect(_leftRegion.x + _leftRegion.width + _region.height, _region.y, rightWidth, _region.height);
            _rightInnerRegion = _rightRegion.ContractedBy(1f);
            
            _middleRegion = new Rect(_leftRegion.x + _leftRegion.width, _region.y, _region.height, _region.height).ContractedBy(1f);
            _middleInnerRect = _middleRegion.ContractedBy(2f);
        }

        public void Draw()
        {
            IPoll currentPoll = PollManager.Instance.CurrentPoll;

            if (currentPoll == null || currentPoll.Options.Length < 2)
            {
                return;
            }

            GUI.DrawTexture(_region, Textures.WindowAtlas);
            
            DrawLeftOption(currentPoll.Options[0]);
            DrawRightOption(currentPoll.Options[1]);
            DrawEmblem();
        }

        private void DrawLeftOption([NotNull] IOption option)
        {
            var leftTextRect = new Rect(_leftRegion.x, _leftRegion.y + _leftRegion.height, _leftRegion.width, Mathf.FloorToInt(_leftRegion.height * 0.33f));

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
            var rightTextRect = new Rect(_rightRegion.x, _rightRegion.y + _rightRegion.height, _rightRegion.width, Mathf.FloorToInt(_rightRegion.height * 0.33f));

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
    }
}
