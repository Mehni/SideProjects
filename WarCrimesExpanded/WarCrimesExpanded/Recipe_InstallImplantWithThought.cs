using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WarCrimesExpanded
{
    public class Recipe_InstallImplantWithThought : Recipe_InstallImplant
    {
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);
            pawn.needs.mood.thoughts.memories.TryGainMemory(WCE_DefOf.WCE_TorturedMe, billDoer);
        }
    }
}
