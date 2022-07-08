﻿// MIT License
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
using System.Collections.Generic;
using ToolkitExt.Api.Interfaces;

namespace ToolkitExt.Core.Models
{
    public class Option : IOption
    {
        private readonly HashSet<string> _voters = new HashSet<string>();

        /// <inheritdoc/>
        public Guid Id { get; set; }

        /// <inheritdoc/>
        public int Votes { get; private set; }

        /// <inheritdoc/>
        public string Label { get; set; }

        /// <inheritdoc/>
        public string Tooltip { get; set; }

        /// <inheritdoc/>
        public Action ChosenAction { get; set; }

        /// <inheritdoc/>
        public void RegisterVote(string userId)
        {
            lock (_voters)
            {
                if (_voters.Add(userId))
                {
                    Votes++;
                }
            }
        }

        /// <inheritdoc/>
        public bool UnregisterVote(string userId)
        {
            lock (_voters)
            {
                if (!_voters.Remove(userId))
                {
                    return false;
                }

                Votes--;

                return true;
            }
        }

        /// <inheritdoc/>
        public void ClearVotes()
        {
            lock (_voters)
            {
                _voters.Clear();
            }

            Votes = 0;
        }
    }
}
