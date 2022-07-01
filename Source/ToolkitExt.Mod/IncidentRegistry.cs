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
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RimWorld;
using ToolkitExt.Api;
using ToolkitExt.Core;
using ToolkitExt.Core.Extensions;
using ToolkitExt.Core.Models;
using Verse;

namespace ToolkitExt.Mod
{
    /// <summary>
    ///     A class for housing <see cref="IncidentDef"/>s.
    /// </summary>
    public static class IncidentRegistry
    {
        private static readonly RimLogger Logger = new RimLogger("ToolkitIncidents");
        private static readonly HashSet<IncidentItem> IncidentDefs = new HashSet<IncidentItem>();
        private static volatile bool _isSyncing;

        static IncidentRegistry()
        {
            var builder = new StringBuilder();

            foreach (IncidentDef incident in DefDatabase<IncidentDef>.AllDefs)
            {
                if (Add(incident))
                {
                    continue;
                }

                if (!incident.TryGetMod(out string name))
                {
                    name = "void.unknown";
                }

                builder.Append($"  - {name}:{incident.defName}\n");
            }

            if (builder.Length <= 0)
            {
                return;
            }

            builder.Insert(0, "The following incidents could not be registered because they were already present, or the mod that added them could not be located:\n");
            Logger.Warn(builder.ToString());
        }

        /// <summary>
        ///     Adds an incident to the registry.
        /// </summary>
        /// <param name="incident">The incident to add</param>
        /// <returns>Whether the incident was added</returns>
        /// <remarks>
        ///     You generally shouldn't need to call this method as
        ///     incidents are added automatically.
        /// </remarks>
        public static bool Add([NotNull] IncidentDef incident)
        {
            IncidentItem item = incident.ToItem();

            return item != null && IncidentDefs.Add(item);
        }

        /// <summary>
        ///     Removes an incident from the registry.
        /// </summary>
        /// <param name="item">The incident to remove</param>
        /// <returns>Whether the incident was removed</returns>
        public static bool Remove(IncidentItem item) => IncidentDefs.Remove(item);

        public static void Sync()
        {
            if (_isSyncing)
            {
                Logger.Warn("Sync requested, but a sync is already in progress...");

                return;
            }

            Task.Run(async () => await SyncIncidentsAsync()).ConfigureAwait(false);
        }

        private static async Task SyncIncidentsAsync()
        {
            _isSyncing = true;

            var synced = false;

            try
            {
                synced = await BackendClient.Instance.UpdateIncidentsAsync(new List<IncidentItem>(IncidentDefs));
            }
            catch (Exception e)
            {
                Logger.Error("Could not sync incidents", e);
            }

            _isSyncing = false;

            Logger.Info($"Incidents synced? {synced.ToStringYesNo()}");
        }
    }
}
