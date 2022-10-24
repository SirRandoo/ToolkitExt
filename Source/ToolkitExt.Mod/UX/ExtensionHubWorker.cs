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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using ToolkitExt.Core.Workers;
using UnityEngine;
using Verse;

namespace ToolkitExt.Mod.UX
{
    [HarmonyPatch]
    [UsedImplicitly]
    [StaticConstructorOnStartup]
    public class ExtensionHubWorker : MainButtonWorker_ToggleTab
    {
        private static readonly JifWorker SpinnerWorker = JifWorker.Load("UI/ToolboxSheet");

        public ExtensionHubWorker()
        {
            Instance = this;
        }

        public bool Spinning => SpinnerWorker.Running;

        public static ExtensionHubWorker Instance { get; private set; }

        /// <inheritdoc/>
        public override void DoButton(Rect rect)
        {
            DoButtonInternal(rect);
        }
        
        private static void DrawIcon(Rect position, Texture image)
        {
            if (Instance is { Spinning: true })
            {
                SpinnerWorker.Draw(position);
            }
            else
            {
                GUI.DrawTexture(position, image);
            }
        }

        [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        [HarmonyPatch(typeof(MainButtonWorker), nameof(MainButtonWorker.DoButton))]
        private void DoButtonInternal(Rect rect)
        {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                MethodInfo drawCall = AccessTools.Method(typeof(GUI), nameof(GUI.DrawTexture), new[] { typeof(Rect), typeof(Texture) });

                foreach (CodeInstruction instruction in instructions)
                {
                    if (instruction.Is(OpCodes.Call, drawCall))
                    {
                        yield return CodeInstruction.Call(typeof(ExtensionHubWorker), nameof(DrawIcon));

                        continue;
                    }

                    yield return instruction;
                }
            }

            _ = Transpiler(null);
        }
    }
}
