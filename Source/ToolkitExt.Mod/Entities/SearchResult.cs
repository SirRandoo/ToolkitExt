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
using JetBrains.Annotations;
using SirRandoo.CommonLib.Helpers;
using UnityEngine;
using Verse;

namespace ToolkitExt.Mod.Entities
{
    public sealed class SearchResult : IEquatable<SearchResult>
    {
        private float _descriptionHeightCache;
        private float _heightCache;
        private float _titleHeightCache;
        private float _widthCache;

        public SearchResult(SearchIndexEntry index)
        {
            Index = index;
        }

        public SearchIndexEntry Index { get; }

        /// <inheritdoc/>
        public bool Equals(SearchResult other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return ReferenceEquals(this, other) || Equals(Index, other.Index);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => ReferenceEquals(this, obj) || (obj is SearchResult other && Equals(other));

        /// <inheritdoc/>
        public override int GetHashCode() => Index != null ? Index.GetHashCode() : 0;

        public static bool operator ==([CanBeNull] SearchResult left, [CanBeNull] SearchResult right) => Equals(left, right);

        public static bool operator !=([CanBeNull] SearchResult left, [CanBeNull] SearchResult right) => !Equals(left, right);

        public float GetHeight(float width)
        {
            if (Mathf.Abs(width - _widthCache) < 0.001f)
            {
                return _heightCache;
            }

            _widthCache = width;
            _titleHeightCache = Text.CalcHeight(Index.Title, width);

            if (!string.IsNullOrEmpty(Index.Description))
            {
                _descriptionHeightCache = Text.CalcHeight(Index.Description, width);
            }

            _heightCache = _titleHeightCache + _descriptionHeightCache;

            return _heightCache;
        }

        public void Draw(Rect region)
        {
            if (Index == null)
            {
                UiHelper.Label(region.AtZero(), "This search entry was deleted.", TextAnchor.UpperLeft);

                GUI.EndGroup();

                return;
            }

            var listing = new Listing_Standard();
            listing.Begin(region.AtZero());

            UiHelper.Label(listing.GetRect(_titleHeightCache), Index.Title);

            if (!string.IsNullOrEmpty(Index.Description))
            {
                listing.DrawDescription(Index.Description);
            }

            if (Widgets.ButtonInvisible(region))
            {
                Index.OnClick.Invoke();
            }

            listing.End();
        }
    }
}
