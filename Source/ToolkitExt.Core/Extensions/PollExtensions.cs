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

using System.Collections.Generic;
using JetBrains.Annotations;
using ToolkitExt.Api.Interfaces;
using Verse;

namespace ToolkitExt.Core.Extensions
{
    /// <summary>
    ///     A class for extending <see cref="IPoll"/>s without forcing
    ///     inheritors into implementing a method.
    /// </summary>
    public static class PollExtensions
    {
        /// <summary>
        ///     Gets the most voted for option(s).
        /// </summary>
        /// <param name="poll">The poll to get the options from</param>
        /// <returns>The most voted for option(s)</returns>
        [ItemNotNull]
        public static IEnumerable<IOption> GetMostVoted([NotNull] this IPoll poll)
        {
            var highest = 0;

            foreach (IOption option in poll.Options)
            {
                if (option.Votes > highest)
                {
                    highest = option.Votes;
                }
            }

            foreach (IOption option in poll.Options)
            {
                if (option.Votes == highest)
                {
                    yield return option;
                }
            }
        }

        /// <summary>
        ///     Gets one choice from the list of options with the most votes.
        /// </summary>
        /// <param name="poll">The poll to get the winning option from</param>
        /// <returns>A randomly selected winner from the most voted for options</returns>
        [CanBeNull]
        public static IOption GetWinningOption([NotNull] this IPoll poll)
        {
            var container = new List<IOption>(GetMostVoted(poll));
            int count = container.Count;

            if (count == 1)
            {
                return container[0];
            }

            if (count <= 0)
            {
                return null;
            }

            return container.TryRandomElement(out IOption option) ? option : null;
        }
    }
}
