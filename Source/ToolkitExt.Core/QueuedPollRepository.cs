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
using ToolkitExt.Core.Models;
using Verse;

namespace ToolkitExt.Core
{
    [StaticConstructorOnStartup]
    public static class QueuedPollRepository
    {
        private static readonly List<RawQueuedPoll> QueuedPolls = new List<RawQueuedPoll>();

        [NotNull]
        public static List<RawQueuedPoll> AllPolls
        {
            get
            {
                lock (QueuedPolls)
                {
                    return new List<RawQueuedPoll>(QueuedPolls);
                }
            }
        }

        public static void Add(RawQueuedPoll poll)
        {
            lock (QueuedPolls)
            {
                QueuedPolls.Add(poll);
            }
        }

        public static void Remove(RawQueuedPoll poll)
        {
            lock (QueuedPolls)
            {
                QueuedPolls.Remove(poll);
            }
        }

        public static void Remove(int pollId)
        {
            lock (QueuedPolls)
            {
                for (int i = QueuedPolls.Count - 1; i >= 0; i--)
                {
                    if (QueuedPolls[i].Id == pollId)
                    {
                        QueuedPolls.RemoveAt(i);
                    }
                }
            }
        }

        [CanBeNull]
        public static RawQueuedPoll GetNext()
        {
            lock (QueuedPolls)
            {
                return QueuedPolls.Count > 0 ? QueuedPolls[0] : null;
            }
        }

        public static bool HasNext()
        {
            lock (QueuedPolls)
            {
                return QueuedPolls.Count > 0;
            }
        }
    }
}
