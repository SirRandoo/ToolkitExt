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

namespace ToolkitExt.Api.Interfaces
{
    /// <summary>
    ///     Represents the rough outline of a poll choice.
    /// </summary>
    public interface IChoice
    {
        /// <summary>
        ///     The unique identifier of the choice.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        ///     The number of people who've voted for this choice.
        /// </summary>
        public int Votes { get; }
        
        /// <summary>
        ///     The label used for displaying the choice to the end user.
        /// </summary>
        public string Label { get; set; }
        
        /// <summary>
        ///     The tooltip for displaying the choice to the end user.
        /// </summary>
        /// <remarks>
        ///     If no tooltip was supplied, this value will be <c>null</c>.
        /// </remarks>
        [CanBeNull] public string Tooltip { get; set; }
        
        public Action ChosenAction { get; set; }

        /// <summary>
        ///     Registers a vote from the user.
        /// </summary>
        /// <param name="userId">The id of the user who voted</param>
        public void RegisterVote(string userId);
        
        /// <summary>
        ///     Unregisters a vote from the user.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns>Whether the user was removed</returns>
        /// <remarks>
        ///     While you can use this method, it's advised not to as the
        ///     extension doesn't support switching votes. This method purely
        ///     exists on the off chance that the poll's votes don't align with
        ///     what the EBS returned.
        /// </remarks>
        public bool UnregisterVote(string userId);
    }
}
