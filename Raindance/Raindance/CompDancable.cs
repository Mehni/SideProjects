using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Raindance
{
    public class CompDancable : ThingComp
    {
        public int lastDanceTick = -1;

        private Pawn lastDancer;

        public bool DancedNow
        {
            get
            {
                return Find.TickManager.TicksGame - this.lastDanceTick <= 1 && this.lastDancer != null && this.lastDancer.Spawned;
            }
        }

        public Pawn ManningPawn
        {
            get
            {
                if (!this.DancedNow)
                {
                    return null;
                }
                return this.lastDancer;
            }
        }

        //public CompProperties_Dancable Props
        //{
        //    get
        //    {
        //        return (CompProperties_Dancable)this.props;
        //    }
        //}

        public void DanceForATick(Pawn pawn)
        {
            this.lastDanceTick = Find.TickManager.TicksGame;
            this.lastDancer = pawn;
            pawn.mindState.lastMannedThing = this.parent;
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
        {
            if (pawn.RaceProps.ToolUser)
            {
                if (pawn.CanReserveAndReach(this.parent, PathEndMode.InteractionCell, Danger.Deadly, 1, -1, null, false))
                {
                    FloatMenuOption opt = new FloatMenuOption("RainDance".Translate(), delegate
                    {
                        Job job = new Job(RaindanceDefOf.RainDance, this.parent);
                        pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }, MenuOptionPriority.Default, null, null, 0f, null, null);
                    yield return opt;
                }
            }
        }
    }    
}
