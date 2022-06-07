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

using System.Collections.Generic;
using JetBrains.Annotations;
using ToolkitExt.Api.Interfaces;

namespace ToolkitExt.Core
{
    public class PollManager
    {
        private readonly Queue<IPoll> _polls = new Queue<IPoll>();
        private IPoll _current;

        [CanBeNull]
        public IPoll CurrentPoll
        {
            get
            {
                if (_current == null)
                {
                    NextPoll();
                }

                return _current;
            }
        }

        public void Queue(IPoll poll)
        {
            _polls.Enqueue(poll);
        }

        public void NextPoll()
        {
            if (_current != null || !_polls.TryDequeue(out _current))
            {
                return;
            }
            
            // TODO: Send the poll to the EBS
        }
    }
}
