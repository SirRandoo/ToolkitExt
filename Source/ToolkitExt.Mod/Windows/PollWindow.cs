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
        private readonly IPoll _poll;
        private float _captionHeight;
        private float _timer;

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
            _timer = (float)(poll.EndedAt - poll.StartedAt).TotalMilliseconds;

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
                var titleRect = new Rect(0f, 0f, inRect.width, _captionHeight);

                GUI.BeginGroup(titleRect);
                
                GameFont cache = Text.Font;
                Text.Font = ExtensionMod.Settings.Polls.LargeText ? GameFont.Medium : GameFont.Small;
                Widgets.Label(titleRect, _poll.Caption);
                Text.Font = cache;

                GUI.EndGroup();
            }

            Rect contentRect = new Rect(0f, _captionHeight, inRect.width, inRect.height - _captionHeight).ContractedBy(4f);
            
            GUI.BeginGroup(contentRect);
            DrawChoices(contentRect.AtZero());
            GUI.EndGroup();
            
            GUI.EndGroup();
        }

        public void DrawChoices(Rect region)
        {
            GameFont cache = Text.Font;
            Text.Font = ExtensionMod.Settings.Polls.LargeText ? GameFont.Medium : GameFont.Small;
            
            for (var i = 0; i < _poll.Choices.Length; i++)
            {
                IChoice choice = _poll.Choices[i];

                var lineRect = new Rect(0f, Text.LineHeight * i, region.width, Text.LineHeight);
                
            }
        }
    }
}
