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
using Verse;

namespace ToolkitExt.Core.Extensions
{
    public static class CollectionExtensions
    {
        public static bool TryRandomElementWeighted<T>([NotNull] this IList<T> list, Func<T, float> selector, out T element)
        {
            var total = 0f;

            foreach (T item in list)
            {
                total += selector(item);
            }

            float weight = Rand.Range(0f, total);

            var cumulative = 0f;
            foreach (T item in list)
            {
                cumulative += selector(item);

                if (weight > cumulative)
                {
                    continue;
                }

                element = item;
                return true;
            }

            element = default;

            return false;
        }
    }
}
