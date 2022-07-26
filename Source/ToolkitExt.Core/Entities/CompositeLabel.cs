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
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace ToolkitExt.Core.Entities
{
    public class CompositeLabel
    {
        private readonly List<Token> _tokens = new List<Token>();

        private void AddToken(Rect region, string word)
        {
            var token = new Token { Segment = word, IsLink = Uri.IsWellFormedUriString(word, UriKind.Absolute), Region = region };
            _tokens.Add(token);
            
            Log.Message($"Added {token.ToString()}");
        }

        public void Draw()
        {
            // at some point, this mess should be expanded to include text anchoring.
            Color oldColor = GUI.color;

            foreach (Token token in _tokens)
            {
                if (token.IsLink)
                {
                    GUI.color = Mouse.IsOver(token.Region) ? new Color(0.75f, 0.73f, 1f) : new Color(0.49f, 0.67f, 1f);
                }

                Widgets.Label(token.Region, token.Segment);

                if (token.IsLink)
                {
                    Widgets.DrawLineHorizontal(token.Region.x, token.Region.y + token.Region.height - 4f, token.Region.width);
                    GUI.color = oldColor;
                }

                if (token.IsLink && Widgets.ButtonInvisible(token.Region))
                {
                    Application.OpenURL(token.Segment);
                }
            }
        }

        [NotNull]
        public static CompositeLabel Compile(Rect region, [NotNull] string content)
        {
            var label = new CompositeLabel();
            float spaceWidth = Text.CalcSize(" ").x;
            string[] words = content.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            float totalWords = words.Length;

            var x = 0f;
            var y = 0f;
            var width = 0f;
            var builder = new StringBuilder();

            for (var index = 0; index < words.Length; index++)
            {
                string word = words[index];
                Vector2 size = Text.CalcSize(word);
                bool atEnd = index >= totalWords - 1;
                bool exceeded = x + width + size.x > region.width;
                bool hitLink = Uri.IsWellFormedUriString(word, UriKind.Absolute);
                bool dumpBuilder = hitLink || exceeded;
                bool shouldAddSpace = hitLink || !atEnd;

                if (dumpBuilder)
                {
                    label.AddToken(new Rect(x, y, width, Text.LineHeight), builder.ToString());
                    builder.Clear();
                }

                if (exceeded)
                {
                    x = 0f;
                    y += Text.LineHeight;
                    width = 0f;
                }

                if (hitLink)
                {
                    var rect = new Rect(x + width, y, size.x, Text.LineHeight);
                    label.AddToken(rect, word);

                    width += size.x;
                }
                else
                {
                    builder.Append(word);
                    width += size.x;
                }

                if (shouldAddSpace)
                {
                    width += spaceWidth;
                    builder.Append(" ");
                }
            }

            return label;
        }

        private struct Token
        {
            public string Segment { get; set; }
            public bool IsLink { get; set; }
            public Rect Region { get; set; }

            /// <inheritdoc />
            public override string ToString()
            {
                return $@"Token[Segment=""{Segment}"", IsLink={IsLink.ToStringYesNo()}, Region={Region.ToString()}]";
            }
        }
    }
}
