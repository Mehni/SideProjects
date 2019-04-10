using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace ManhuntingDinos
{
    public class IncidentWorker_DinosaurManhunterPack : IncidentWorker
    {
        //purely vanilla
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!DinosaurManhunterPackIncidentUtility.TryFindManhunterAnimalKind(parms.points, map.Tile, out PawnKindDef pawnKindDef)) return false;
            if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 intVec, map, CellFinder.EdgeRoadChance_Animal)) return false;

            List<Pawn> list = DinosaurManhunterPackIncidentUtility.GenerateAnimals(pawnKindDef, map.Tile, parms.points * 1.4f);
            Rot4 rot = Rot4.FromAngleFlat((map.Center - intVec).AngleFlat);
            for (int i = 0; i < list.Count; i++)
            {
                Pawn pawn = list[i];
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(intVec, map, 10);
                GenSpawn.Spawn(pawn, loc, map, rot);
                pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
                pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + Rand.Range(GenDate.TicksPerDay, 135000);
            }
            Find.LetterStack.ReceiveLetter("LetterLabelManhunterPackArrived".Translate(), "ManhunterPackArrived".Translate(pawnKindDef.GetLabelPlural()), LetterDefOf.ThreatBig, list[0]);
            Find.TickManager.slower.SignalForceNormalSpeedShort();
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Important);
            return true;
        }
    }
}
