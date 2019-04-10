using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace MomCanWePlayOnTheHoloscope
{
    public class JobDriver_PlayHoloscope : JobDriver_SitFacingBuilding
    {
        protected override void ModifyPlayToil(Toil toil)
        {
            base.ModifyPlayToil(toil);
            toil.WithEffect(() => DefDatabase<EffecterDef>.GetNamed("PlayHoloscope"), () => base.TargetA.Thing.OccupiedRect().ClosestCellTo(this.pawn.Position));
        }
    }
}
