using Harmony;
using Verse;
using RimWorld;

namespace Nocturnalis
{
    using JetBrains.Annotations;

    [UsedImplicitly]
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("mehni.rimworld.nocturnalis");

            harmony.Patch(AccessTools.Method(typeof(JobGiver_GetRest), nameof(JobGiver_GetRest.GetPriority)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(LetSleepingDogsWake_PreFix)));
        }

        private static bool LetSleepingDogsWake_PreFix(ref float __result, Pawn pawn)
        {
            if (!pawn.def.HasModExtension<SleepingPatterns>())
                return true;

            if (pawn.RaceProps.Humanlike)
                return true;

            if (pawn.timetable != null)
                return true;

            SleepingPatterns modExt = pawn.def.GetModExtension<SleepingPatterns>();

            int hourOfDay = GenLocalDate.HourOfDay(pawn);

            TimeAssignmentDef timeAssignmentDef = hourOfDay >= modExt.napTime[modExt.sleepingHabit].First && hourOfDay <= modExt.napTime[modExt.sleepingHabit].Second ? TimeAssignmentDefOf.Anything : TimeAssignmentDefOf.Sleep;

            float curLevel = pawn.needs.rest.CurLevel;

            if (timeAssignmentDef == TimeAssignmentDefOf.Anything)
            {
                if (curLevel < 0.3f)
                    __result = 8f;

                __result = 0f;
                return false;
            }

            if (timeAssignmentDef == TimeAssignmentDefOf.Sleep)
            {
                if (curLevel < RestUtility.FallAsleepMaxLevel(pawn))
                    __result = 8f;

                __result = 0f;
                return false;
            }

            return true;
        }
    }
}
