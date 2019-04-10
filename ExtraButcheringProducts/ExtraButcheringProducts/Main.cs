using System.Collections.Generic;
using Verse;
using Harmony;


namespace ExtraButcheringProducts
{
    [StaticConstructorOnStartup]
    internal static class ExtraButcheringProducts
    {
        static ExtraButcheringProducts()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("mehni.rimworld.framework.extrabutchering");

            harmony.Patch(AccessTools.Method(typeof(Thing), nameof(Thing.ButcherProducts)),
                postfix: new HarmonyMethod(typeof(ExtraButcheringProducts), nameof(MakeButcherProducts_PostFix)));
        }

        private static void MakeButcherProducts_PostFix(Thing __instance, ref IEnumerable<Thing> __result, Pawn butcher, float efficiency)
        {
            if (__instance.TryGetComp<CompSpecialButcherChance>() is CompSpecialButcherChance comp)
            {
                if (comp.Props.butcherProducts.NullOrEmpty()) return;

                foreach (ThingDefCountWithChanceClass item in comp.Props.butcherProducts)
                {
                    if (Rand.Chance(item.chance))
                    {
                        ThingDefCountWithChanceClass ta = new ThingDefCountWithChanceClass { thingDef = item.thingDef, count = item.count };

                        int count = GenMath.RoundRandom(ta.count * efficiency);
                        if (count > 0)
                        {
                            Thing t = ThingMaker.MakeThing(ta.thingDef);
                            t.stackCount = count;
                            __result = __result.Add(t);
                        }
                    }
                }
            }
        }
    }
}
