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
using System.Collections.Generic;
using JetBrains.Annotations;
using ToolkitExt.Api.Interfaces;
using Verse;

namespace ToolkitExt.Api.Registries
{
    [StaticConstructorOnStartup]
    public static class PollFactoryRegistry
    {
        private static readonly List<IPollFactory> Factories = new List<IPollFactory>();

        static PollFactoryRegistry()
        {
            foreach (Type type in typeof(IPollFactory).AllSubclassesNonAbstract())
            {
                Register(type);
            }
        }

        public static IEnumerable<IPollFactory> AllFactories => Factories;
        public static IEnumerable<IPollFactory> AllFactoriesRandom => Factories.InRandomOrder();

        public static void Register([NotNull] Type type)
        {
            if (!(Activator.CreateInstance(type) is IPollFactory factory))
            {
                return;
            }

            Factories.Add(factory);
        }

        public static void Unregister([NotNull] Type type)
        {
            for (int i = Factories.Count - 1; i >= 0; i--)
            {
                if (Factories[i].GetType() == type)
                {
                    Factories.RemoveAt(i);
                }
            }
        }
    }
}
