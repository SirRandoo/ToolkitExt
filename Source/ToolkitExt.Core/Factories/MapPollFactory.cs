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
using RimWorld;
using ToolkitExt.Api.Interfaces;
using ToolkitExt.Core.Models;
using Verse;

namespace ToolkitExt.Core.Factories
{
    public class MapPollFactory : IPollFactory
    {
        private readonly IncidentDef[] _incidents;

        public MapPollFactory()
        {
            var container = new List<IncidentDef>();

            foreach (IncidentDef incident in DefDatabase<IncidentDef>.AllDefs)
            {
                if (incident.TargetTagAllowed(IncidentTargetTagDefOf.Map_Misc))
                {
                    container.Add(incident);
                }
            }

            _incidents = container.ToArray();
        }

        /// <inheritdoc/>
        [NotNull]
        public string Caption => "Which map event should happen?";

        /// <inheritdoc/>
        [NotNull]
        public IOptionContext[] CreateOptions()
        {
            var container = new List<IOptionContext>();

            foreach (IncidentDef incident in _incidents.InRandomOrder())
            {
                if (container.Count >= 2)
                {
                    break;
                }
                
                IncidentParms @params = StorytellerUtility.DefaultParmsNow(incident.category, Find.AnyPlayerHomeMap);

                if (incident.Worker.CanFireNow(@params))
                {
                    container.Add(new OptionContext { Incident = incident, Params = @params });
                }
            }

            return container.ToArray();
        }
    }
}
