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

namespace ToolkitExt.Mod
{
    public static class HubMessageLog
    {
        private const int MessageLimit = 250;
        private static readonly List<string> LOG = new List<string>();

        [NotNull]
        public static IEnumerable<string> AllMessages
        {
            get
            {
                lock (LOG)
                {
                    return new List<string>(LOG);
                }
            }
        }

        public static void LogMessage(string message)
        {
            lock (LOG)
            {
                LOG.Add(message);

                if (LOG.Count > MessageLimit)
                {
                    LOG.RemoveAt(0);
                }
            }
        }

        public static void ClearMessages()
        {
            lock (LOG)
            {
                LOG.Clear();
            }
        }
    }
}
