using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Harmony;

namespace Doom2016
{
    public class IncidentWorker_DOOMSpawner : IncidentWorker
    {

        protected override bool CanFireNowSub(IncidentParms incidentParms)
        {
            Map map = (Map)incidentParms.target;
            //TODO: Make cows less evil.
            return map.listerThings.ThingsOfDef(ThingDefOf.Cow).Count <= 0 && !map.gameConditionManager.ConditionIsActive(GameConditionDefOf.PsychicDrone);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            //todo: find a faction properly

            parms.faction = Find.FactionManager.AllFactions.Where(f => !f.IsPlayer && !f.defeated && f.HostileTo(Faction.OfPlayer)).RandomElement();

            Map map = (Map)parms.target;
            int num = 0;
            int countToSpawn = 1;
            IntVec3 cell = IntVec3.Invalid;
            float shrapnelDirection = Rand.Range(0f, 360f);
            for (int i = 0; i < countToSpawn; i++)
            {
                if (!CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.CrashedShipPartIncoming, map, out IntVec3 intVec, 14, default(IntVec3), -1, false, true, true, true))
                {
                    break;
                }
                Building_CrashedShipPart building_CrashedShipPart = (Building_CrashedShipPart)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("DOOM2016ShipPart"));
                building_CrashedShipPart.SetFaction(parms.faction);
                building_CrashedShipPart.GetComp<CompSpawnDOOMOnDamaged>().pointsLeft = Mathf.Max(parms.points * 0.9f, 300f);
                Skyfaller skyfaller = SkyfallerMaker.MakeSkyfaller(ThingDefOf.CrashedShipPartIncoming, building_CrashedShipPart);
                skyfaller.shrapnelDirection = shrapnelDirection;
                GenSpawn.Spawn(skyfaller, intVec, map);
                num++;
                cell = intVec;
            }
            if (num > 0)
            {
                base.SendStandardLetter(new TargetInfo(cell, map, false), parms.faction, new string[0]);
            }
            return num > 0;
        }
    }
}