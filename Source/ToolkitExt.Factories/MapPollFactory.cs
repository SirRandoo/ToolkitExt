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

using JetBrains.Annotations;
using RimWorld;
using ToolkitExt.Api.Interfaces;
using ToolkitExt.Factories;
using Verse;

namespace ToolkitExt.Core.Factories
{
    public class MapPollFactory : IncidentPollFactory
    {
        /// <inheritdoc/>
        [NotNull]
        protected override string GetCaption(IOption[] options) => "Which map event should happen?";

        /// <inheritdoc/>
        protected override IncidentParms GetParams([NotNull] IncidentDef incident) => StorytellerUtility.DefaultParmsNow(incident.category, Find.AnyPlayerHomeMap);

        /// <inheritdoc/>
        protected override bool IsIncidentValid([NotNull] IncidentDef incident)
        {
            if (incident.TargetTagAllowed(IncidentTargetTagDefOf.Map_PlayerHome))
            {
                return true;
            }

            if (incident.TargetTagAllowed(IncidentTargetTagDefOf.Map_Misc))
            {
                return true;
            }

            return incident.TargetTagAllowed(IncidentTargetTagDefOf.Map_RaidBeacon);
        }
    }
}
