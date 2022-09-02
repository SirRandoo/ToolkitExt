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
using JetBrains.Annotations;
using RimWorld;
using ToolkitExt.Api.DefOfs;
using Verse;
#pragma warning disable CS0618

namespace ToolkitExt.Mod.Workers
{
    public class AngryTurtleWorker : IncidentWorker_ManhunterPack
    {
        /// <inheritdoc />
        protected override bool CanFireNowSub(IncidentParms parms) => true;

        /// <inheritdoc />
        protected override bool TryExecuteWorker([NotNull] IncidentParms parms)
        {
            var map = (Map)parms.target;
            IntVec3 center = parms.spawnCenter;

            if (!center.IsValid && !RCellFinder.TryFindRandomPawnEntryCell(out center, map, CellFinder.EdgeRoadChance_Animal))
            {
                return false;
            }

            List<Pawn> pawns = ManhunterPackIncidentUtility.GenerateAnimals(ExtPawnKindDefOf.Tortoise, map.Tile, parms.points, Rand.Range(1, 3));
            Rot4 rotation = Rot4.FromAngleFlat((map.Center - center).AngleFlat);
            
            foreach (Pawn pawn in pawns)
            {
                IntVec3 position = CellFinder.RandomClosewalkCellNear(center, map, 10);
                Thing tortoise = GenSpawn.Spawn(pawn, position, map, rotation);
                QuestUtility.AddQuestTag(tortoise, parms.questTag);
                pawn.health.AddHediff(HediffDefOf.Scaria);
                pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
                pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame * Rand.Range(60000, 120000);
            }

            SendStandardLetter(
                "LetterLabelManhunterPackArrived".TranslateSimple(),
                "ManhunterPackArrived".Translate(ExtPawnKindDefOf.Tortoise.GetLabelPlural()),
                LetterDefOf.ThreatSmall,
                parms,
                pawns[0]
            );
            
            Find.TickManager.slower.SignalForceNormalSpeedShort();
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Important);

            return true;
        }
    }
}
