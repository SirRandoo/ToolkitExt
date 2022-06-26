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
using ToolkitExt.Core.Extensions;
using ToolkitExt.Core.Models;
using Verse;

namespace ToolkitExt.Core.Factories
{
    /// <summary>
    ///     An abstract class for making incident polls.
    /// </summary>
    public abstract class IncidentPollFactory : IPollFactory
    {
        private protected readonly IncidentDef[] IncidentDefs;

        protected IncidentPollFactory()
        {
            IncidentDefs = GetIncidents();
        }

        /// <inheritdoc cref="IPollFactory.Create"/>
        [CanBeNull]
        public IPoll Create()
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
            var container = new List<IOption>();

            foreach (IncidentDef incident in IncidentDefs)
            {
                if (container.Count >= 2)
                {
                    break;
                }

                IncidentParms @params = GetParams(incident);

                if (incident.Worker.CanFireNow(@params))
                {
                    container.Add(CreateOption(incident, @params));
                }
            }

            return container.ToArray();
        }

        [NotNull]
        private IncidentDef[] GetIncidents()
        {
            var container = new List<IncidentDef>();

            foreach (IncidentDef incident in DefDatabase<IncidentDef>.AllDefs)
            {
                if (IsIncidentValid(incident))
                {
                    container.Add(incident);
                }
            }

            return container.ToArray();
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
    }
}
