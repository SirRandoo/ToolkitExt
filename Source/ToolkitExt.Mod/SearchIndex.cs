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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using HarmonyLib;
using JetBrains.Annotations;
using ToolkitExt.Api;
using ToolkitExt.Mod.Entities;
using ToolkitExt.Mod.Models;
using Verse;

namespace ToolkitExt.Mod
{
    /// <summary>
    ///     A class for housing search entries.
    ///     Developers are to register entries with this class if they would
    ///     like users to be able to search for their content.
    /// </summary>
    /// <remarks>
    ///     Due to the GUI framework RimWorld uses, there's no maintainable
    ///     way to index certain content to use for a "search everywhere"
    ///     feature, so an index of searchable content is a suitable
    ///     alternative.
    /// </remarks>
    [StaticConstructorOnStartup]
    public static class SearchIndex
    {
        private static readonly XmlSerializer XmlSerializer = new XmlSerializer(typeof(SearchIndexesXml));
        private static readonly RimLogger Logger = new RimLogger("ToolkitExt.SearchIndexes");
        private static readonly List<SearchIndexEntry> Entries = new List<SearchIndexEntry>();

        static SearchIndex()
        {
            foreach (SearchIndexXml index in GatherSearchIndexes())
            {
                SearchIndexEntry converted = ConvertFromXml(index);

                if (converted == null)
                {
                    Logger.Warn($"Could not convert {index.Slug} to an index entry; ignoring...");

                    continue;
                }

                AddEntry(converted);
            }
        }

        [CanBeNull]
        private static SearchIndexEntry ConvertFromXml([NotNull] SearchIndexXml index)
        {
            MethodInfo method;

            try
            {
                method = AccessTools.Method(index.Method);
            }
            catch (TypeInitializationException e)
            {
                Logger.Error($@"The search index ""{index.Slug}"" contains a malformed method; skipping...", e);

                return null;
            }

            if (method == null)
            {
                Logger.Warn($"The method {index.Method} could not be found; skipping...");

                return null;
            }

            var keyWords = new string[index.KeyWords.Count];

            for (var i = 0; i < index.KeyWords.Count; i++)
            {
                SearchKeyWordXml word = index.KeyWords[i];
                keyWords[i] = word.KeyWord;
            }

            string title = string.IsNullOrEmpty(index.TitleKey) ? index.Title : index.TitleKey.TranslateSimple();
            string description = string.IsNullOrEmpty(index.DescriptionKey) ? index.Description : index.DescriptionKey.TranslateSimple();
            
            Logger.Debug($"{index.Slug} title: {title}");
            Logger.Debug($"{index.Slug} description: {description}");

            if (!string.IsNullOrEmpty(title))
            {
                return SearchIndexEntry.Create(index.Slug, title, description, () => method.Invoke(method.DeclaringType, Array.Empty<object>()), keyWords);
            }

            Logger.Warn($"{index.Slug} did not have a valid title; skipping...");

            return null;
        }

        private static void AddEntry([CanBeNull] SearchIndexEntry entry)
        {
            if (entry == null)
            {
                Logger.Warn("Passed a null entry; ignoring...");

                return;
            }

            lock (Entries)
            {
                Entries.Add(entry);
            }

            Logger.Debug($"Added {entry.Slug} to list of search entries...");
        }

        [ItemNotNull]
        private static IEnumerable<SearchIndexXml> GatherSearchIndexes()
        {
            foreach (string folder in ExtensionMod.Instance.Content.foldersToLoadDescendingOrder)
            {
                string indexPath = Path.Combine(folder, "Indexes");
                Logger.Debug(indexPath);

                if (!Directory.Exists(indexPath))
                {
                    Logger.Warn($"The index path {indexPath} does not exist; ignoring...");

                    continue;
                }

                foreach (SearchIndexXml index in LoadSearchIndexes(indexPath))
                {
                    if (index == null)
                    {
                        Logger.Warn($"Could not load search indexes at {indexPath} ignoring...");

                        continue;
                    }

                    yield return index;
                }
            }
        }

        [ItemCanBeNull]
        private static IEnumerable<SearchIndexXml> LoadSearchIndexes([NotNull] string filePath)
        {
            foreach (string path in Directory.EnumerateFiles(filePath, "*.xml", SearchOption.AllDirectories))
            {
                Logger.Debug(path);

                SearchIndexesXml contents = null;

                try
                {
                    using (FileStream file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        contents = (SearchIndexesXml)XmlSerializer.Deserialize(file);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"Could not deserialize {path}", e);
                }

                if (contents == null)
                {
                    Logger.Warn($"The contents of {path} were null");

                    continue;
                }

                foreach (SearchIndexXml index in contents.Indexes)
                {
                    yield return index;
                }
            }
        }

        /// <summary>
        ///     Adds a search entry to the list of indexes.
        /// </summary>
        /// <param name="slug">The unique identifier for the entry</param>
        /// <param name="title">The title of the entry</param>
        /// <param name="onClick">
        ///     The action to perform when the entry is clicked
        ///     by the user
        /// </param>
        public static void AddIndex(string slug, [NotNull] string title, Action onClick)
        {
            lock (Entries)
            {
                Entries.Add(SearchIndexEntry.Create(slug, title, onClick));
            }
        }

        /// <summary>
        ///     Adds a search entry to the list of indexes.
        /// </summary>
        /// <param name="slug">The unique identifier for the entry</param>
        /// <param name="title">The title of the entry</param>
        /// <param name="description">The description of the entry</param>
        /// <param name="onClick">
        ///     The action to perform when the entry is clicked
        ///     by the user
        /// </param>
        public static void AddIndex(string slug, string title, string description, Action onClick)
        {
            lock (Entries)
            {
                Entries.Add(SearchIndexEntry.Create(slug, title, description, onClick));
            }
        }

        /// <summary>
        ///     Adds a search entry to the list of indexes.
        /// </summary>
        /// <param name="slug">The unique identifier for the entry</param>
        /// <param name="title">The title of the entry</param>
        /// <param name="description">The description of the entry</param>
        /// <param name="onClick">
        ///     The action to perform when the entry is clicked
        ///     by the user
        /// </param>
        /// <param name="keyWords">
        ///     A collection of words that describe the search
        ///     entry
        /// </param>
        public static void AddIndex(string slug, string title, string description, Action onClick, [NotNull] params string[] keyWords)
        {
            lock (Entries)
            {
                Entries.Add(SearchIndexEntry.Create(slug, title, description, onClick, keyWords));
            }
        }

        /// <summary>
        ///     Removes a search entry from the list of indexes.
        /// </summary>
        /// <param name="slug">The unique identifier for the search entry</param>
        /// <returns>The number of entries removed. This should typically be 1.</returns>
        public static int RemoveIndex(string slug)
        {
            lock (Entries)
            {
                return Entries.RemoveAll(e => string.Equals(e.Slug, slug, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        ///     Finds all search entries that could match the given query.
        ///     The query is compared against the title, description, and key
        ///     words of entries.
        /// </summary>
        /// <param name="query">The query to search for</param>
        /// <returns>A collection of search entries could match the given query</returns>
        [NotNull]
        public static ReadOnlyCollection<SearchResult> Find([NotNull] string query)
        {
            var container = new HashSet<SearchResult>();

            foreach (SearchResult entry in FindByTitle(query))
            {
                container.Add(entry);
            }

            foreach (SearchResult entry in FindByDescription(query))
            {
                if (container.Contains(entry))
                {
                    continue;
                }

                container.Add(entry);
            }

            foreach (SearchResult entry in FindByKeyWords(query))
            {
                if (container.Contains(entry))
                {
                    continue;
                }

                container.Add(entry);
            }

            return new ReadOnlyCollection<SearchResult>(new List<SearchResult>(container));
        }


        /// <summary>
        ///     Finds all search entries that could match the given query.
        /// </summary>
        /// <param name="query">The query to search for</param>
        /// <returns>A collection of search entries could match the given query</returns>
        [NotNull]
        public static ReadOnlyCollection<SearchResult> FindByTitle([NotNull] string query)
        {
            var container = new List<SearchResult>();

            lock (Entries)
            {
                foreach (SearchIndexEntry entry in Entries)
                {
                    if (entry.TryTitleMatch(out SearchResult result, query))
                    {
                        container.Add(result);
                    }
                }
            }

            return container.AsReadOnly();
        }


        /// <summary>
        ///     Finds all search entries that could match the given query.
        /// </summary>
        /// <param name="query">The query to search for</param>
        /// <returns>A collection of search entries could match the given query</returns>
        [NotNull]
        public static ReadOnlyCollection<SearchResult> FindByDescription([NotNull] string query)
        {
            var container = new List<SearchResult>();
            string[] words = query.Split(' ');

            lock (Entries)
            {
                foreach (SearchIndexEntry entry in Entries)
                {
                    if (string.IsNullOrEmpty(entry.Description))
                    {
                        continue;
                    }

                    if (entry.DescriptionMatchesQuery(out SearchResult result, query))
                    {
                        container.Add(result);
                    }

                    if (entry.DescriptionMatchesQuery(out result, words))
                    {
                        container.Add(result);
                    }
                }
            }

            return container.AsReadOnly();
        }


        /// <summary>
        ///     Finds all search entries that could match the given query.
        /// </summary>
        /// <param name="query">The query to search for</param>
        /// <returns>A collection of search entries could match the given query</returns>
        [NotNull]
        public static ReadOnlyCollection<SearchResult> FindByKeyWords([NotNull] string query)
        {
            var container = new List<SearchResult>();
            string[] words = query.Split(' ');

            lock (Entries)
            {
                foreach (SearchIndexEntry entry in Entries)
                {
                    if (entry.QueryIsKeyWord(out SearchResult result, words))
                    {
                        container.Add(result);
                    }
                }
            }

            return container.AsReadOnly();
        }
    }
}
