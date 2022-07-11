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
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Verse;

namespace ToolkitExt.Core.Extensions
{
    /// <summary>
    ///     A gross, hacky solution to using asynchronous code in a Unity environment.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class TaskExtensions
    {
        static TaskExtensions()
        {
            MainThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            MainThreadFactory = new TaskFactory(
                CancellationToken.None,
                TaskCreationOptions.DenyChildAttach,
                TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.ExecuteSynchronously,
                MainThreadScheduler
            );
        }

        private static TaskScheduler MainThreadScheduler { get; }
        private static TaskFactory MainThreadFactory { get; }

        /// <summary>
        ///     Executes a <see cref="Func{TResult}"/> on the Unity thread, and returns the function's result.
        /// </summary>
        /// <param name="func">The function to execute</param>
        /// <typeparam name="T">The return type of the function</typeparam>
        /// <returns>The result of the function</returns>
        public static async Task<T> OnMainAsync<T>([NotNull] this Func<T> func) => await MainThreadFactory.StartNew(func);

        /// <summary>
        ///     Executes an <see cref="Action"/> on the Unity thread.
        /// </summary>
        /// <param name="func">The action to execute</param>
        public static async Task OnMainAsync([NotNull] this Action func) => await MainThreadFactory.StartNew(func);

        /// <summary>
        ///     Executes a <see cref="Task"/> on the Unity thread.
        /// </summary>
        /// <param name="task">The task to execute</param>
        public static async Task OnMainAsync([NotNull] this Task task)
        {
            Task runner = await MainThreadFactory.StartNew(async () => await task);

            await runner;
        }

        /// <summary>
        ///     Executes a <see cref="Task{TResult}"/> on the Unity thread.
        /// </summary>
        /// <param name="task">The task to execute</param>
        /// <typeparam name="T">The return type of the task</typeparam>
        /// <returns>The result of the task</returns>
        public static async Task<T> OnMainAsync<T>([NotNull] this Task<T> task)
        {
            Task<T> runner = await MainThreadFactory.StartNew(async () => await task);

            return await runner;
        }

        /// <summary>
        ///     Attempts to get a <see cref="ThingComp"/> from the given thing.
        /// </summary>
        /// <param name="thing">The thing to get the comp from</param>
        /// <typeparam name="T">The <see cref="Type"/> of the comp being returned</typeparam>
        /// <returns>The comp instance of <see cref="T"/>, or <c>null</c></returns>
        public static async Task<T> TryGetCompAsync<T>([NotNull] this Thing thing) where T : ThingComp => await MainThreadFactory.StartNew(thing.TryGetComp<T>);
    }
}
