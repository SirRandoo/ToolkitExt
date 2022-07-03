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

using JetBrains.Annotations;
using RimWorld;
using ToolkitExt.Core.Models;
using Verse;

namespace ToolkitExt.Core.Extensions
{
    public static class DefExtensions
    {
        [ContractAnnotation("=> true, name: notnull; => false, name: null")]
        public static bool TryGetMod([NotNull] this Def def, out string name)
        {
            name = def.modContentPack?.Name;

            return name != null;
        }

        [CanBeNull]
        public static IncidentItem ToItem([NotNull] this IncidentDef incident)
        {
            if (!incident.TryGetMod(out string name))
            {
                return null;
            }

            return new IncidentItem(incident.defName)
            {
                ModId = name,
                Label = incident.LabelCap,
                Description = incident.letterLabel ?? null,
            };
        }
    }
}
