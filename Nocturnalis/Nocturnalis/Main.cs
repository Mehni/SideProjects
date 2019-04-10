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

            bool shouldSleep = hourOfDay >= modExt.napTime[modExt.sleepingHabit].First
                            && hourOfDay <= modExt.napTime[modExt.sleepingHabit].Second
                            ? false
                            : true;

            float curLevel = pawn.needs.rest.CurLevel;

            if (!shouldSleep)
            {
                __result = curLevel < 0.3f ? 8f : 0f;

                return false;
            }

            if (shouldSleep)
            {
                __result = curLevel < RestUtility.FallAsleepMaxLevel(pawn) ? 8f : 0f;

                return false;
            }

            return true;
        }
    }
}
