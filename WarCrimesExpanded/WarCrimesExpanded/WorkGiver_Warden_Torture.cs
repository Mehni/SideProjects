using RimWorld;
using Verse;
using Verse.AI;

namespace WarCrimesExpanded
{
    public class WorkGiver_Warden_Torture : WorkGiver_Warden
    {
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!ShouldTakeCareOfPrisoner(pawn, t))
                return null;

            if (!(t is Pawn prisoner) || prisoner.guest.interactionMode != WCE_DefOf.WCE_TortureResistance || pawn.story.WorkTagIsDisabled(WorkTags.Violent))
                return null;

            if (prisoner.guest.ScheduledForInteraction && !prisoner.Downed && pawn.CanReserve(t))
                return new Job(WCE_DefOf.WCE_PrisonerTorture, t);

            return null;
        }
    }
}
