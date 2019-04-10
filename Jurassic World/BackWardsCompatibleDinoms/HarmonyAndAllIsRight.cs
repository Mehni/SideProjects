using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;

namespace BackWardsCompatibleDinoms
{

    public class HarmonyAndAllIsRight : Mod
    {

        private static bool doneItOnce = false;

        static HarmonyAndAllIsRight()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("mehni.rimworld.rjw-jrw-whatsthedifferenceamirite");

            harmony.Patch(AccessTools.Method(typeof(BackCompatibility), nameof(BackCompatibility.BackCompatibleDefName)), null,
                new HarmonyMethod(typeof(HarmonyAndAllIsRight), nameof(AreYouBraveEnough)));
        }

        public HarmonyAndAllIsRight(ModContentPack content) : base(content)
        {
        }

        private static void AreYouBraveEnough(ref string __result, Type defType, string defName, bool forDefInjections = false)
        {
            //if not a thingdef, GTFO
            if (defType != typeof(ThingDef))
                return;
            //if original already found something, GTFO.
            if (__result != defName)
                return;

            ModContentPack mcp = LoadedModManager.RunningModsListForReading.Find(x => x.Name == "Jurassic Rimworld 1.0");
            Mod mod = LoadedModManager.ModHandles.FirstOrFallback(x => x.Content == mcp);

            //if mod not found or if already done, GTFO. (Will continue on if world == null)
            if (mod != null && mod.GetSettings<BackWardsCompatibility>().alreadyDone
                                  .Contains(Find.World?.ConstantRandSeed ?? -1))
            {
                return;
            }

            //game wasn't saved with JRW. GTFO.
            if (!ScribeMetaHeaderUtility.LoadedModsMatchesActiveMods(out string loadedModsSummary, out string runningModsSummary))
            {
                if (!loadedModsSummary.Contains("Jurassic Rimworld 1.0"))
                    return;
            }

#if DEBUG
            if (ScrewingAroundWithGeneratingDefNames()) return;
#endif
            //change actual defname. Or at least look it up.
            __result = BackwardsCompatibleDefName.BackWardsDefname(defName);

            //GTFO if not in a state to continue.
            if (mod == null || Find.World == null || doneItOnce)
                return;

            //queue event to save to settings
            LongEventHandler.ExecuteWhenFinished(() =>
                                                 {
                                                     if (mod.GetSettings<BackWardsCompatibility>().alreadyDone.Contains(Find.World.ConstantRandSeed))
                                                         return;

                                                     mod.GetSettings<BackWardsCompatibility>().alreadyDone.Add(Find.World.ConstantRandSeed);
                                                     mod.WriteSettings();
                                                 });

            //only need to queue up the long event *once*
            doneItOnce = true;
        }

        public override void WriteSettings()
        {
            //make sure settings are only saved once.
            GetSettings<BackWardsCompatibility>().alreadyDone = GetSettings<BackWardsCompatibility>().alreadyDone.Distinct().ToList();
            base.WriteSettings();
        }

        /// <summary>
        /// Was used to generate the BackwardsCompatibleDefname class.
        /// </summary>
        /// <returns></returns>
        private static bool ScrewingAroundWithGeneratingDefNames()
        {
            StringBuilder thong = new StringBuilder();

            var defs = LoadedModManager.RunningModsListForReading.FirstOrDefault(x => x?.Name == "Jurassic Rimworld 1.0")
                                      ?.AllDefs;

            if (defs == null)
            {
                Log.Message("mod not found");
                var smth = LoadedModManager.RunningModsListForReading.FirstOrDefault(x => x?.Name == "Jurassic Rimworld 1.0");
                Log.Message($"Does it have a name? {smth?.Name}");
                return true;
            }

            foreach (var def in defs)
            {
                if (!(def is ThingDef dinoDef))
                    continue;

                if (dinoDef.description != null && dinoDef.description.StartsWith("-- >"))
                    thong.Append($"if (defName == \"{def.defName}\")\n{{\n    return \"JRW_{def.defName}\";\n}}\n\n");

                if (dinoDef.IsMeat)
                    thong.Append($"if (defName == \"{def.defName}\")\n{{\n    return \"Meat_JRW_{def.defName.Substring(5)}\";\n}}\n\n");

                if (dinoDef.IsCorpse)
                    thong.Append($"if (defName == \"{def.defName}\")\n{{\n    return \"Corpse_JRW_{def.defName.Substring(7)}\";\n}}\n\n");
            }

            Log.ErrorOnce(thong.ToString(), 847461264);
            return false;
        }


    }


    public class BackWardsCompatibility : ModSettings
    {
        public List<int> alreadyDone = new List<int>();

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref alreadyDone, "alreadyDinofied", LookMode.Value);
        }
    }
}
