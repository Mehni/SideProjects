using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace TOBE_Fishing
{
    public class WorkGiver_Fish : WorkGiver_DoBill
    {
        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            if (thing.def.AllRecipes.Any(x => x.HasModExtension<RecipeOptions>()) && 
                pawn.equipment?.Primary?.GetComp<CompFishingRod>() != null)
            {
                Job job = base.JobOnThing(pawn, thing, forced);
                if (job != null && job.def == JobDefOf.DoBill)
                    job.def = TOBE_Fishing_DefOf.TOBE_DoBillWithRod;

                return job;
            }
            return null;
        }
    }
}
