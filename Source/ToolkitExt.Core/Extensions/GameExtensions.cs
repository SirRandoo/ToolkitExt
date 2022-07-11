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
using System.Threading.Tasks;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace ToolkitExt.Core.Extensions
{
    public static class GameExtensions
    {
        /// <summary>
        ///     Attempts to get a <see cref="ThingComp"/> from the given thing.
        /// </summary>
        /// <param name="thing">The thing to get the comp from</param>
        /// <typeparam name="T">The <see cref="Type"/> of the comp being returned</typeparam>
        /// <returns>
        ///     The comp instance of <see cref="T"/>, or <c>null</c>
        /// </returns>
        public static async Task<T> TryGetCompAsync<T>([NotNull] this Thing thing) where T : ThingComp => await TaskExtensions.OnMainAsync(thing.TryGetComp<T>);

        public static async Task<bool> CanFireNowAsync([NotNull] this IncidentWorker worker, IncidentParms parms)
        {
            return await TaskExtensions.OnMainAsync(() => worker.CanFireNow(parms));
        }
    }
}
