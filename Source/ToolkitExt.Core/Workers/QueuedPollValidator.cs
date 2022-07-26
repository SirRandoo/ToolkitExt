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
using System.Threading.Tasks;
using JetBrains.Annotations;
using RimWorld;
using ToolkitExt.Api.Interfaces;
using ToolkitExt.Core.Extensions;
using ToolkitExt.Core.Helpers;
using ToolkitExt.Core.Models;
using ToolkitExt.Core.Registries;
using Verse;

namespace ToolkitExt.Core.Workers
{
    public static class QueuedPollValidator
    {
        [ItemNotNull]
        public static async Task<ValidationResult> ValidateAsync([NotNull] MinimalRawQueuedPoll poll)
        {
            var options = new List<IOption>();

            foreach (RawQueuedOption option in poll.Options)
            {
                IncidentItem incident = IncidentRegistry.Get(option.ModId, option.DefName);

                if (incident?.Def == null)
                {
                    return new ValidationResult { Valid = false, ErrorString = $@"The option ""{option.Label}"" doesn't exist in the game." };
                }

                IncidentParms @params = await GetIncidentParamsAsync(incident.Def).OnMainAsync();

                if (@params == null)
                {
                    return new ValidationResult { Valid = false, ErrorString = $@"The option ""{option.Label}"" isn't possible at this time." };
                }

                options.Add(incident.Def.ToOption(@params));
            }

            return new ValidationResult { Valid = true, Poll = new QueuedPoll { Caption = poll.Title, Length = poll.Length, Id = poll.Id, Options = options.ToArray() } };
        }

        [ItemCanBeNull]
        private static async Task<IncidentParms> GetIncidentParamsAsync([NotNull] IncidentDef incident)
        {
            if (incident.TargetTagAllowed(IncidentTargetTagDefOf.World))
            {
                return await GetWorldIncidentParamsAsync(incident);
            }

            //  If we don't support a modded target tag, we'll just return null.
            return incident.TargetsMap() ? await GetMapIncidentParamsAsync(incident) : null;
        }

        [ItemCanBeNull]
        private static async Task<IncidentParms> GetWorldIncidentParamsAsync([NotNull] IncidentDef incident)
        {
            IncidentParms @params = await StorytellerUtilityAsync.DefaultParamsNowAsync(incident.category, Find.World);
            bool canFireNow = await incident.Worker.CanFireNowAsync(@params);

            return !canFireNow ? null : @params;
        }

        [ItemCanBeNull]
        private static async Task<IncidentParms> GetMapIncidentParamsAsync([NotNull] IncidentDef incident)
        {
            IncidentParms @params = await StorytellerUtilityAsync.DefaultParamsNowAsync(incident.category, Find.AnyPlayerHomeMap);
            bool canFireNow = await incident.Worker.CanFireNowAsync(@params);

            return !canFireNow ? null : @params;
        }

        public sealed class ValidationResult
        {
            public bool Valid { get; set; }
            public string ErrorString { get; set; }
            public IPoll Poll { get; set; }
        }
    }
}
