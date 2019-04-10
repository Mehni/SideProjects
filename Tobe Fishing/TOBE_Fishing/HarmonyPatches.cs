using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using Verse;
using RimWorld;
using UnityEngine;

namespace TOBE_Fishing
{

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static readonly List<ThingDef> allFish;

        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("mehni.rimworld.TOBE_Fishing");

            harmony.Patch(AccessTools.Method(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(MakeRecipeProductsFish_PostFix)));

            allFish = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(x => x.HasModExtension<FishProperties>());
        }

        public static void MakeRecipeProductsFish_PostFix(ref IEnumerable<Thing> __result, RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient, IBillGiver billGiver)
        {
            if (!recipeDef.HasModExtension<RecipeOptions>())
                return;

            if (!(billGiver.GetWorkgiver()?.Worker is WorkGiver_Fish))
                return;

            worker.CurJob.RecipeDef.Worker.ConsumeIngredient(worker.CurJob.targetB.Thing, recipeDef, worker.Map);

            FishGrade grade = FishGradeUtility.GenerateFishGrade(worker.equipment.Primary, worker, recipeDef.GetModExtension<RecipeOptions>().increasedFishQuality);

            if (!allFish.Where(td => td.GetModExtension<FishProperties>().grade == grade)
                .TryRandomElementByWeight(gtd => gtd.GetModExtension<FishProperties>().biomes.FirstOrDefault(b => b.biome == worker.Map.Biome).mtbDays, out ThingDef caughtFish))
            {
                Log.Message($"no fish of grade {grade} found for biome {worker.Map.Biome}");
            }

            //var potentialFish = allFish.Where(td => td.GetModExtension<FishProperties>().grade == grade);
            //Log.Message($"potentialfish with grade {grade}: {potentialFish.Count()}");
            //var potFish = allFish.Where(gtd => gtd.GetModExtension<FishProperties>().biomes.Any(b => b.biome == worker.Map.Biome));

            //Log.Message($"potFish with biome {worker.Map.Biome}: {potFish.Count()}");
            //var distinct = potFish.Concat(potentialFish).Distinct();
            //Log.Message($"distinct {distinct.Count()}");


            Thing spawnFish = null;

            if (Rand.Chance(recipeDef.GetModExtension<RecipeOptions>().successChance) && caughtFish != null)
            {
                spawnFish = ThingMaker.MakeThing(caughtFish);
                spawnFish.stackCount = __result?.FirstOrDefault()?.stackCount ?? 1;
            }
            else
            {
                MoteText youSuckAtFishing = (MoteText)ThingMaker.MakeThing(ThingDefOf.Mote_Text, null);
                youSuckAtFishing.SetVelocity(Rand.Range(5, 35), Rand.Range(0.42f, 0.45f));
                youSuckAtFishing.text = "TextMote_HarvestFailed".Translate();
                youSuckAtFishing.textColor = Color.white;
                spawnFish = youSuckAtFishing;
            }

            __result = new List<Thing>{spawnFish};
        }
    }
}
