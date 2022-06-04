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
        ///     The time this poll ended at.
        /// </summary>
        /// <remarks>
        ///     Implementors should ensure this property isn't set before the
        ///     poll is started, and that an additional dither is applied to
        ///     accomodate users with poor connections.
        /// </remarks>
        public DateTime EndedAt { get; set; }

        /// <summary>
        ///     The time this poll started at.
        /// </summary>
        /// <remarks>
        ///     This property is automatically set when the poll by the relevant
        ///     manager.
        /// </remarks>
        public DateTime StartedAt { get; set; }
        
        /// <summary>
        ///     The choices viewers can vote on.
        /// </summary>
        public IChoice[] Choices { get; set; }
        
        /// <summary>
        ///     The duration of the poll in minutes.
        /// </summary>
        public int Duration { get; }
    }
}
