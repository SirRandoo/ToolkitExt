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
using JetBrains.Annotations;

namespace ToolkitExt.Mod.Entities
{
    /// <summary>
    ///     Represents a entry for searchable content.
    /// </summary>
    public sealed class SearchIndexEntry : IEquatable<SearchIndexEntry>
    {
        private SearchIndexEntry(string slug, [NotNull] string title, [CanBeNull] string description, Action onClick, [NotNull] params string[] keyWords)
        {
            Slug = slug;
            Title = title;
            Description = description;
            OnClick = onClick;
            KeyWords = new List<string>(keyWords);
        }

        /// <summary>
        ///     The unique identifier of the entry.
        /// </summary>
        public string Slug { get; }

        /// <summary>
        ///     The title of the entry
        /// </summary>
        public string Title { get; }

        /// <summary>
        ///     The action to invoke when the user clicks on this search entry.
        /// </summary>
        public Action OnClick { get; }

        /// <summary>
        ///     The optional description of the search entry.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     A list of key words for the given search entry.
        /// </summary>
        public List<string> KeyWords { get; }

        /// <inheritdoc/>
        public bool Equals(SearchIndexEntry other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Slug == other.Slug && Title == other.Title && Description == other.Description && Equals(KeyWords, other.KeyWords);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => ReferenceEquals(this, obj) || (obj is SearchIndexEntry other && Equals(other));

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Slug != null ? Slug.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Title != null ? Title.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (KeyWords != null ? KeyWords.GetHashCode() : 0);

                return hashCode;
            }
        }

        /// <summary>
        ///     Returns whether the given query could match the search entry.
        /// </summary>
        /// <param name="query">The query to check</param>
        [CanBeNull]
        public SearchResult MatchesQuery([NotNull] string query)
        {
            if (TryTitleMatch(out SearchResult result, query))
            {
                return result;
            }

            if (DescriptionMatchesQuery(out result, query))
            {
                return result;
            }

            return QueryIsKeyWord(out result, query) ? result : null;
        }

        /// <summary>
        ///     Returns whether the given deconstructed query could match the
        ///     search entry's title.
        /// </summary>
        /// <param name="result">
        ///     The search result for the query, or null if the
        ///     query did not match the title
        /// </param>
        /// <param name="tokens">The deconstructed query</param>
        [ContractAnnotation("=> true, result: notnull; => false, result: null")]
        public bool TryTitleMatch(out SearchResult result, [NotNull] params string[] tokens)
        {
            foreach (string token in tokens)
            {
                if (!Title.Contains(token))
                {
                    continue;
                }

                result = new SearchResult(this);

                return true;
            }

            result = null;

            return false;
        }

        /// <summary>
        ///     Returns whether the given deconstructed query could match the
        ///     search entry's description.
        /// </summary>
        /// <param name="result">
        ///     The search result for the query, or null if the
        ///     query did not match the title
        /// </param>
        /// <param name="tokens">The deconstructed query</param>
        [ContractAnnotation("=> true, result: notnull; => false, result: null")]
        public bool DescriptionMatchesQuery(out SearchResult result, params string[] tokens)
        {
            if (string.IsNullOrEmpty(Description))
            {
                result = null;

                return false;
            }

            foreach (string token in tokens)
            {
                if (Description.IndexOf(token, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                result = new SearchResult(this);

                return true;
            }

            result = null;

            return false;
        }

        /// <summary>
        ///     Returns whether the given query could match the search entry's
        ///     key words.
        /// </summary>
        /// <param name="result">
        ///     The search result for the query, or null if the
        ///     query did not match the title
        /// </param>
        /// <param name="tokens">The deconstructed query to check</param>
        [ContractAnnotation("=> true, result: notnull; => false, result: null")]
        public bool QueryIsKeyWord(out SearchResult result, params string[] tokens)
        {
            if (KeyWords.Count <= 0)
            {
                result = null;

                return false;
            }

            foreach (string token in tokens)
            {
                foreach (string word in KeyWords)
                {
                    if (word.IndexOf(token, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    result = new SearchResult(this);
                    return true;
                }
            }

            result = null;

            return false;
        }

        /// <summary>
        ///     Constructs a new search entry.
        /// </summary>
        /// <param name="slug">The unique identifier for the search entry</param>
        /// <param name="title">The title of the search entry</param>
        /// <param name="onClick">
        ///     The action to invoke when the user clicks on
        ///     the search entry
        /// </param>
        [NotNull]
        public static SearchIndexEntry Create(string slug, [NotNull] string title, Action onClick) => Create(slug, title, null, onClick, Array.Empty<string>());

        /// <summary>
        ///     Constructs a new search entry.
        /// </summary>
        /// <param name="slug">The unique identifier for the search entry</param>
        /// <param name="title">The title of the search entry</param>
        /// <param name="description">The description of the search entry</param>
        /// <param name="onClick">
        ///     The action to invoke when the user clicks on
        ///     the search entry
        /// </param>
        [NotNull]
        public static SearchIndexEntry Create(string slug, [NotNull] string title, string description, Action onClick) =>
            Create(slug, title, description, onClick, Array.Empty<string>());

        /// <summary>
        ///     Constructs a new search entry.
        /// </summary>
        /// <param name="slug">The unique identifier for the search entry</param>
        /// <param name="title">The title of the search entry</param>
        /// <param name="description">The description of the search entry</param>
        /// <param name="onClick">
        ///     The action to invoke when the user clicks on
        ///     the search entry
        /// </param>
        /// <param name="keyWords">A collection of key words for the search entry</param>
        [NotNull]
        public static SearchIndexEntry Create(string slug, [NotNull] string title, string description, Action onClick, [NotNull] params string[] keyWords) =>
            new SearchIndexEntry(slug, title, description, onClick, keyWords);
    }
}
