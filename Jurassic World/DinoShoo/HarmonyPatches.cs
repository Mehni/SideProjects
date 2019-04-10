using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
using Harmony;

namespace DinoShoo
{
    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("Mehni.RimWorld.ForSerpy.BigDinosLeave");

            harmony.Patch(AccessTools.Method(typeof(ThinkNode_ConditionalExitTimedOut), "Satisfied"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(DinoShoo_PostFix)), null);
        }

        private static void DinoShoo_PostFix(ref Pawn pawn)
        {
            if (pawn.mindState.exitMapAfterTick == -99999 && pawn.RaceProps.baseBodySize >= 5f && pawn.Faction == null)
            {
                pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + Rand.RangeInclusive(90000, 150000);
            }
        }
    }
}
