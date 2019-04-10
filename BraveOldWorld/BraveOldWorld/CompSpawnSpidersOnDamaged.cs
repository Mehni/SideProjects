using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.Sound;

// i was also going to ask how easy it would be to duplicate the SpawnMechanoidsOnDamage comp to make one for spiders? i think i'd like to try and make an event that acts similar where it's a hole in the ground and if you disturb it you get angry spooders

namespace BraveOldWorld
{
    public class CompSpawnSpidersOnDamaged : ThingComp
    {
        public float pointsLeft;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref pointsLeft, "spooderPointsLeft", 0f);
        }

        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(dinfo, out absorbed);
            if (absorbed)
            {
                return;
            }
            if (dinfo.Def.harmsHealth)
            {
                float num = (float)parent.HitPoints - dinfo.Amount;
                if ((num < (float)parent.MaxHitPoints * 0.98f && dinfo.Instigator != null && dinfo.Instigator.Faction != null) || num < (float)parent.MaxHitPoints * 0.9f)
                {
                    pointsLeft = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, parent.MapHeld).points;
                    TrySpawnSpiders();
                }
            }
            absorbed = false;
        }

        private void TrySpawnSpiders()
        {
            if (pointsLeft <= 0f || !parent.Spawned)
            {
                return;
            }
            try
            {
                while (pointsLeft > 0f && (from def in DefDatabase<PawnKindDef>.AllDefs
                                           where def.RaceProps.body.defName.ToLower() == "TarantulaLike" && def.combatPower <= pointsLeft
                                           select def).TryRandomElement(out PawnKindDef spooder) && (from cell in GenAdj.CellsAdjacent8Way(parent)
                                                                                                     where CanSpawnSpiderAt(cell)
                                                                                                     select cell).TryRandomElement(out IntVec3 spot))
                {
                    PawnGenerationRequest request = new PawnGenerationRequest(spooder, forceGenerateNewPawn: true);
                    Pawn pawn = PawnGenerator.GeneratePawn(request);
                    if (!GenPlace.TryPlaceThing(pawn, spot, parent.Map, ThingPlaceMode.Near))
                    {
                        Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                        break;
                    }
                    pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
                    pointsLeft -= pawn.kindDef.combatPower;
                }
            }
            finally
            {
                pointsLeft = 0f;
            }
            SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(parent.Map);
        }

        private bool CanSpawnSpiderAt(IntVec3 c)
            => c.Walkable(parent.Map);
    }
}

