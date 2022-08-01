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
using RimWorld;
using ToolkitExt.Api;
using ToolkitExt.Api.Interfaces;
using ToolkitExt.Core.Extensions;
using ToolkitExt.Core.Models;
using Verse;

namespace ToolkitExt.Factories
{
    /// <summary>
    ///     An abstract class for making incident polls.
    /// </summary>
    public abstract class IncidentPollFactory : WeightedPollFactory
    {
        private readonly IncidentEntry[] _incidentDefs;
        private static readonly RimLogger Logger = new RimLogger("IncidentPollFactory");

        protected IncidentPollFactory()
        {
            _incidentDefs = GetIncidents();
        }

        /// <inheritdoc cref="IPollFactory.Create"/>
        [CanBeNull]
        public override IPoll Create()
        {
            IOption[] options = GetOptions();

            return options.Length < 2 ? null : new Poll { Caption = GetCaption(options), Options = options };
        }

        /// <summary>
        ///     Called once per poll to get the current caption of it.
        /// </summary>
        /// <param name="options">The current options of the poll</param>
        protected abstract string GetCaption(IOption[] options);

        /// <summary>
        ///     Called one per incident to get the <see cref="IncidentParms"/>
        ///     for the given incident.
        /// </summary>
        /// <param name="incident">The incident to get params for</param>
        protected abstract IncidentParms GetParams(IncidentDef incident);

        /// <summary>
        ///     Called once per poll to get an array of options.
        /// </summary>
        /// <remarks>
        ///     While you can supply more than 2 options, the extension only
        ///     supports 2. Any additional options will be removed.
        /// </remarks>
        [NotNull]
        protected IOption[] GetOptions()
        {
            foreach (IncidentEntry entry in _incidentDefs)
            {
                float oldWeight = GetWeightFor(entry.Id);
                float weight = GetWeightIncrease(entry.Incident, oldWeight);
                SetWeightFor(entry.Id, weight);
            }

            return GetOptionsInternal();
        }

        [NotNull]
        private IOption[] GetOptionsInternal()
        {
            var loops = 0;
            var containerIndex = 0;
            var container = new IOption[2];

            while (containerIndex < 2)
            {
                if (loops > 10000)
                {
                    return Array.Empty<IOption>();
                }

                if (!_incidentDefs.TryRandomElementByWeight(i => GetWeightFor(i.Id), out IncidentEntry entry))
                {
                    loops++;

                    continue;
                }

                IncidentParms @params = GetParams(entry.Incident);

                if (!entry.Incident.Worker.CanFireNow(@params))
                {
                    loops++;
                    
                    continue;
                }

                container[containerIndex] = entry.Incident.ToOption(@params);
                float weight = GetWeightDecrease(entry.Incident, GetWeightFor(entry.Id));

                SetWeightFor(entry.Id, weight);
                containerIndex++;
                loops++;
            }

            for (var index = 0; index < container.Length; index++)
            {
                IOption option = container[index];
                Logger.Debug($"Option #{index:N0} :: {option.ToStringSafe()}");
            }

            return container;
        }

        [NotNull]
        private IncidentEntry[] GetIncidents()
        {
            var container = new List<IncidentEntry>();

            foreach (IncidentDef incident in DefDatabase<IncidentDef>.AllDefs)
            {
                if (!IsIncidentValid(incident))
                {
                    continue;
                }

                string modName = incident.TryGetMod(out string name) ? name : null;

                if (modName == null)
                {
                    continue;
                }

                var identifier = $"{modName}:{incident.defName}";
                float defaultWeight = GetDefaultWeight(incident);
                SetWeightFor(identifier, defaultWeight);

                container.Add(new IncidentEntry { Id = identifier, Incident = incident });
            }

            return container.ToArray();
        }

        private static float GetDefaultWeight([NotNull] IncidentDef incident)
        {
            float weight = ModLister.RoyaltyInstalled ? incident.baseChanceWithRoyalty : incident.baseChance;

            if (incident.category == IncidentCategoryDefOf.ThreatBig)
            {
                return weight - weight * 0.2f;
            }

            if (incident.category == IncidentCategoryDefOf.ThreatSmall)
            {
                return weight - weight * 0.3f;
            }

            return weight;
        }

        private static float GetWeightIncrease([NotNull] IncidentDef incident, float oldWeight)
        {
            if (incident.category == IncidentCategoryDefOf.ThreatBig)
            {
                return oldWeight + oldWeight * 0.2f;
            }

            if (incident.category == IncidentCategoryDefOf.ThreatSmall)
            {
                return oldWeight + oldWeight * 0.3f;
            }

            return oldWeight + oldWeight * 0.6f;
        }

        private static float GetWeightDecrease([NotNull] IncidentDef incident, float oldWeight)
        {
            if (incident.category == IncidentCategoryDefOf.ThreatBig)
            {
                return oldWeight - oldWeight * 0.9f;
            }

            if (incident.category == IncidentCategoryDefOf.ThreatSmall)
            {
                return oldWeight - oldWeight * 0.8f;
            }

            return oldWeight - oldWeight * 0.7f;
        }

        /// <summary>
        ///     Called once per incident to create an <see cref="IOption"/> from
        ///     it.
        /// </summary>
        /// <param name="incident">The incident to be run with the option wins</param>
        /// <param name="paramz">The parameters of the incident</param>
        /// <returns>
        ///     An <see cref="IOption"/> instance that will run
        ///     <see cref="incident"/> when chosen by the viewers
        /// </returns>
        [NotNull]
        protected virtual IOption CreateOption([NotNull] IncidentDef incident, IncidentParms paramz) => incident.ToOption(paramz);

        /// <summary>
        ///     Called one per <see cref="IncidentDef"/> to determine if it
        ///     should be included in the factory's internal list.
        /// </summary>
        /// <param name="incident">The <see cref="IncidentDef"/> to check</param>
        /// <returns>
        ///     Whether the incident is valid, and can be added to the
        ///     factory's internal list
        /// </returns>
        protected abstract bool IsIncidentValid(IncidentDef incident);

        private sealed class IncidentEntry
        {
            public string Id { get; set; }
            public IncidentDef Incident { get; set; }
        }
    }
}
