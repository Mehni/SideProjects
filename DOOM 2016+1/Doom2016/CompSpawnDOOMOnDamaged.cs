using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace Doom2016
{
    //lazy copy of CompSpawnerMechanoidsOnDamaged
    public class CompSpawnDOOMOnDamaged : ThingComp
    {
        public float pointsLeft;

        private Lord lord;

        private const float DoomDefendRadius = 21f;

        public static readonly string MemoDamaged = "ShipPartDamaged";

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look<Lord>(ref this.lord, "doomDefenseLord", false);
            Scribe_Values.Look<float>(ref this.pointsLeft, "doomPointsLeft", 0f, false);
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
                if (this.lord != null)
                {
                    this.lord.ReceiveMemo(CompSpawnDOOMOnDamaged.MemoDamaged);
                }
                float num = (float)(this.parent.HitPoints - dinfo.Amount);

                //unlike vanilla, you gotta hit this ship harder. Instead of 98% health => 80%. Or a 50% chance. Guaranteed to trigger at 70%.
                if ((num < (float)this.parent.MaxHitPoints * 0.8f && dinfo.Instigator != null && dinfo.Instigator.Faction != null) || num < (float)this.parent.MaxHitPoints * 0.7f)
                {
                    this.TrySpawnDOOOM();
                }
            }
            absorbed = false;
        }

        public void Notify_BlueprintReplacedWithSolidThingNearby(Pawn by)
        {
            if (by.Faction != this.parent.Faction)
            {
                this.TrySpawnDOOOM();
            }
        }

        private void TrySpawnDOOOM()
        {
            if (this.pointsLeft <= 0f)
            {
                return;
            }
            if (!this.parent.Spawned)
            {
                return;
            }
            if (this.lord == null)
            {
                if (!CellFinder.TryFindRandomCellNear(this.parent.Position, this.parent.Map, 5, (IntVec3 c) => c.Standable(this.parent.Map) && this.parent.Map.reachability.CanReach(c, this.parent, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)), out IntVec3 invalid))
                {
                    Log.Error("Found no place for DOOM to defend " + this);
                    invalid = IntVec3.Invalid;
                }
                //default LordJob works just fine for us.
                LordJob_MechanoidsDefendShip lordJob = new LordJob_MechanoidsDefendShip(this.parent, this.parent.Faction, DoomDefendRadius, invalid);
                this.lord = LordMaker.MakeNewLord(this.parent.Faction, lordJob, this.parent.Map, null);
            }
            while ((from def in DefDatabase<PawnKindDef>.AllDefs
                    where /*def.RaceProps.IsMechanoid && def.isFighter */ def.RaceProps.Animal && def.race.HasComp(typeof(CompMilkable)) && def.combatPower <= this.pointsLeft
                    select def).TryRandomElement(out PawnKindDef kindDef))
            {
                if ((from cell in GenAdj.CellsAdjacent8Way(this.parent)
                     where this.CanSpawnDOOOOMAt(cell)
                     select cell).TryRandomElement(out IntVec3 center))
                {
                    Pawn pawn = PawnGenerator.GeneratePawn(kindDef, this.parent.Faction);
                    if (GenPlace.TryPlaceThing(pawn, center, this.parent.Map, ThingPlaceMode.Near, null))
                    {
                        this.lord.AddPawn(pawn);
                        this.pointsLeft -= pawn.kindDef.combatPower;
                        continue;
                    }
                    Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                }
                this.pointsLeft = 0f;
                SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(this.parent.Map);
                return;
            }
        }

        private bool CanSpawnDOOOOMAt(IntVec3 c)
        {
            return c.Walkable(this.parent.Map);
        }
    }
    
}
