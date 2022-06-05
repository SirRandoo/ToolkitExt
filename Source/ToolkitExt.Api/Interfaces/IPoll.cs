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

namespace ToolkitExt.Api.Interfaces
{
    /// <summary>
    ///     An interface outlining the properties required to outline a poll.
    /// </summary>
    public interface IPoll
    {
        /// <summary>
        ///     The unique identifier of the poll.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     The caption of the poll.
        /// </summary>
        /// <remarks>
        ///     This property should be used to describe the poll to the user in
        ///     a short sentence.
        /// </remarks>
        public string Caption { get; set; }

        /// <summary>
        ///     The total number of people that have voted on the poll.
        /// </summary>
        public int TotalVotes { get; }

        /// <summary>
        ///     The time this poll ended at.
        /// </summary>
        /// <remarks>
        ///     This property is automatically set by the poll manager. You
        ///     should not supply a custom end <c>DateTime</c> or the poll may
        ///     never be displayed to viewers.
        /// </remarks>
        public DateTime EndedAt { get; set; }

        /// <summary>
        ///     The time this poll started at.
        /// </summary>
        /// <remarks>
        ///     This property is automatically set by the poll manager. You
        ///     should not supply a custom start <c>DateTime</c> or the poll may
        ///     never be displayed to viewers.
        /// </remarks>
        public DateTime StartedAt { get; set; }

        /// <summary>
        ///     The choices viewers can vote on.
        /// </summary>
        /// <remarks>
        ///     While you can supply several choices, the extension only supports
        ///     two choices. Polls exceeding this limit will be truncated down to
        ///     meet that limit.
        /// </remarks>
        public IChoice[] Choices { get; set; }

        /// <summary>
        ///     The duration of the poll in minutes.
        /// </summary>
        public int Duration { get; }

        /// <summary>
        ///     Registers a vote for the given choice by the given user.
        /// </summary>
        /// <param name="userId">The id of the user who voted</param>
        /// <param name="choiceId">The id of the choice the user voted for</param>
        public void RegisterVote(string userId, Guid choiceId);

        /// <summary>
        ///     Unregisters a vote from the given user.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns>Whether the user was removed from the poll's ballots</returns>
        /// <remarks>
        ///     While you can use this method, it's advised not to as the
        ///     extension doesn't support switching votes. This method purely
        ///     exists on the off chance that the poll's votes don't align with
        ///     what the EBS returned.
        /// </remarks>
        public bool UnregisterVote(string userId);
    }
}
