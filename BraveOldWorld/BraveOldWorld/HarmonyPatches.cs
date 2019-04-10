using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;

// think mostly i'd appreciate a duplicate of the shearing comp/giver that relabels it as threading or something, and the wool growth to be silk growth instead. other than that the ability of being able to only milk venom if a pawn is bonded to that animal.

namespace BraveOldWorld
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("mehni.rimworld.BraveOldWorld");

            harmony.Patch(AccessTools.Method(typeof(CompHasGatherableBodyResource), nameof(CompHasGatherableBodyResource.Gathered)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(BodyResource_Gathered_PostFix)));


            harmony.Patch(AccessTools.Method(typeof(WorkGiver_GatherAnimalBodyResources), nameof(WorkGiver_GatherAnimalBodyResources.HasJobOnThing)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(BodyResource_HasJob_PostFix)));
        }

        private static void BodyResource_HasJob_PostFix(WorkGiver_GatherAnimalBodyResources __instance, Pawn pawn, Thing t, ref bool __result)
        {
            if (!__result)
                return;


            if (t.TryGetComp<CompThreading>() is CompThreading comp)
            {
                if (!pawn.relations.DirectRelationExists(PawnRelationDefOf.Bond, (Pawn)t))
                {
                    __result = false;
                }
            }
        }

        private static void BodyResource_Gathered_PostFix(CompHasGatherableBodyResource __instance, Pawn doer)
        {
            if (!(__instance is CompThreading))
                return;

            if (Rand.Chance(0.1f))
            {
                Pawn spider = (Pawn)__instance.parent;
                Verb verb = null; //will use "default" melee attack
                if (spider.meleeVerbs.GetUpdatedAvailableVerbsList(false).Where(
                    x => (x.verb.tool?.linkedBodyPartsGroup?.defName.ToLower().Contains("fang") ?? false)
                      || (x.verb.tool?.linkedBodyPartsGroup?.defName.ToLower().Contains("teeth") ?? false)
                      || (x.verb.tool?.linkedBodyPartsGroup?.defName.ToLower().Contains("tooth") ?? false))
                   .TryRandomElement(out var verbEntry))
                {
                    verb = verbEntry.verb;
                }
                spider.meleeVerbs.TryMeleeAttack(doer, verb, true);
            }
        }
    }
}
