using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using TOBE_Fishing;
using Harmony;

namespace FishGeneration
{

    public class FishGen : Mod
    {

        public FishGen(ModContentPack content) : base(content)
        {
            HarmonyInstance harmony = HarmonyInstance.Create("mehni.rimworld.fishgeneration");

            harmony.Patch(AccessTools.Method(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve)), 
                postfix: new HarmonyMethod(typeof(FishGen), nameof(PatchImpliedDefs)));

            harmony.Patch(AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.MakeRecipeProductsFish_PostFix)), 
                prefix: new HarmonyMethod(typeof(FishGen), nameof(DoingItBetter)));

            harmony.Patch(AccessTools.Method(typeof(StatWorker), nameof(StatWorker.GetValueUnfinalized)), 
                postfix: new HarmonyMethod(typeof(FishGen), nameof(ApplyFishProps_PostFix)));

            harmony.Patch(AccessTools.Method(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts)),
                          postfix: new HarmonyMethod(typeof(FishGen), nameof(MakeRecipeProductsFish_PostFix)));
        }

        // ReSharper disable once InconsistentNaming
        public static void ApplyFishProps_PostFix(ref float __result, StatWorker __instance, StatDef ___stat, StatRequest req)
        {
            if (!req.HasThing)
                return;

            if (req.Thing.TryGetComp<CompFishProps>() == null)
                return;

            CompFishProps comp = req.Thing.TryGetComp<CompFishProps>();

            if (___stat == comp.mass.stat)
                __result += comp.mass.value;

            if (___stat == comp.nutrition.stat)
                __result += comp.nutrition.value;
        }

        public static bool DoingItBetter(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient, IBillGiver billGiver)
        {
            return recipeDef.defName != "Fishing_Basic_Fish";
        }

        public static void MakeRecipeProductsFish_PostFix(ref IEnumerable<Thing> __result, RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient, IBillGiver billGiver)
        {
            if (!recipeDef.HasModExtension<RecipeOptions>())
                return;

            if (!(billGiver.GetWorkgiver()?.Worker is WorkGiver_Fish))
                return;

            List<ThingDef> allFish = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(x => x.HasModExtension<TOBE_Fishing.FishProperties>());

            worker.CurJob.RecipeDef.Worker.ConsumeIngredient(worker.CurJob.targetB.Thing, recipeDef, worker.Map);

            FishGrade grade = FishGradeUtility.GenerateFishGrade(worker.equipment.Primary, worker, recipeDef.GetModExtension<RecipeOptions>().increasedFishQuality);

            bool InGradeRange(FishGrade gradeIn)
            {
                int delta = grade - gradeIn; // +/- 1
                if (delta >= 2 || delta <= -2)
                    return false;
                return true;
            }

            if (!allFish.Where(td => InGradeRange(td.GetModExtension<FishProperties>().grade))
                .TryRandomElementByWeight(gtd => (gtd.GetModExtension<FishProperties>().biomes.FirstOrDefault(b => b.biome == worker.Map.Biome)?.mtbDays ?? 0), out ThingDef caughtFish))
            {
                Log.Message($"no fish of grade {grade} found for biome {worker.Map.Biome}");
            }

            Thing spawnFish = null;

            if (Rand.Chance(recipeDef.GetModExtension<RecipeOptions>().successChance) && caughtFish != null)
            {
                spawnFish = ThingMaker.MakeThing(caughtFish);
                spawnFish.stackCount = __result?.FirstOrDefault()?.stackCount ?? 1;
                var props = spawnFish.TryGetComp<CompFishProps>();
                props.grade = grade;
                var subGrades = Enum.GetValues(typeof(FishSubGrade));
                props.subGradeMass = (FishSubGrade)subGrades.GetValue(Rand.RangeInclusive(0, subGrades.Length - 1));
                props.subGradeSize = (FishSubGrade)subGrades.GetValue(Rand.RangeInclusiveSeeded(0, subGrades.Length - 1, spawnFish.GetHashCode()));

                float mass = spawnFish.def.GetStatValueAbstract(StatDefOf.Mass);
                float size = (spawnFish.def.GetStatValueAbstract(StatDefOf.Nutrition) * 200) / mass;

                var fishprops = spawnFish.def.GetModExtension<FishProperties>();

                mass *= GetMassMultiplierForMain(fishprops.grade, grade);
                mass *= GetMassMultiplierForSecondary(props.subGradeMass);
                props.mass.value = mass;

                size *= GetSizeMultiplierForMain(fishprops.grade, grade);
                size *= GetSizeMultiplierForSecondary(props.subGradeSize);
                float nutrition = mass * size / 200;

                props.nutrition.value = nutrition;
                //tDef.SetStatBaseValue(StatDefOf.MarketValue, nutrition * 10);
            }
            else
            {
                MoteText youSuckAtFishing = (MoteText)ThingMaker.MakeThing(ThingDefOf.Mote_Text, null);
                youSuckAtFishing.SetVelocity(Rand.Range(5, 35), Rand.Range(0.42f, 0.45f));
                youSuckAtFishing.text = "TextMote_HarvestFailed".Translate();
                youSuckAtFishing.textColor = Color.white;

                youSuckAtFishing.exactPosition = worker.DrawPos;

                spawnFish = youSuckAtFishing;
            }

            __result = new List<Thing> { spawnFish };
        }

        public static void PatchImpliedDefs()
        {
            foreach (ThingDef item in HereWeGoooo())
            {
                DefGenerator.AddImpliedDef(item);
            }
        }

        public static IEnumerable<ThingDef> HereWeGoooo()
        {
            //get all thingDefs with naming scheme X_CC_.
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            char[] FishGrades = new char[] { 'F', 'D', 'C', 'B', 'A', 'S' };

            int count = DefDatabase<ThingDef>.AllDefsListForReading.Count();
            foreach (var item in DefDatabase<ThingDef>.AllDefs.ToList())
            {
                if (Matches(item, FishGrades))
                {
                    ThingDef meat = MeatMaker(item);
                    //FishProperties fishprops = item.GetModExtension<FishProperties>();
                    //foreach (FishGrade grade in Enum.GetValues(typeof(FishGrade)))
                    //{
                    //    if (grade == FishGrade.S)
                    //        continue;

                    //    //Log.Message($"grade {grade} for {item.label}", true);

                    //    int delta = grade - fishprops.grade; // +/- 1
                    //    if (delta >= 2 || delta <= -2)
                    //        continue;

                    //    //Log.Message($"making grade {grade} for {item.label}", true);

                    //    foreach (FishSubGrade subgrade_A in Enum.GetValues(typeof(FishSubGrade)))
                    //    {
                    //        //Log.Message($"{subgrade_A} for {item.label}", true);
                    //        foreach (FishSubGrade subgrade_B in Enum.GetValues(typeof(FishSubGrade)))
                    //        {
                    //            if (string.IsNullOrEmpty(item.label))
                    //                Log.Error($"empty label in {item.defName}");

                    //            string defName = grade + "_" + subgrade_A + subgrade_B + "_" + item.label;

                    //            if (defName == item.defName)
                    //                continue;

                    //            ThingDef tDef = new ThingDef
                    //            {
                    //                label = item.label,
                    //                category = ThingCategory.Item,
                    //                alwaysHaulable = true,
                    //                rotatable = false,
                    //                pathCost = 15,
                    //                drawGUIOverlay = true,
                    //                thingClass = typeof(ThingWithComps),
                    //                tickerType = TickerType.Rare,
                    //                selectable = true,
                    //                altitudeLayer = AltitudeLayer.Item,
                    //                useHitPoints = true,
                    //                defName = defName,
                    //            };

                    //            tDef.graphicData = item.graphicData;
                    //            tDef.modExtensions = item.modExtensions;

                    //            float mass = item.GetStatValueAbstract(StatDefOf.Mass);
                    //            float size = (item.GetStatValueAbstract(StatDefOf.Nutrition) * 200) / mass;

                    //            mass *= GetMassMultiplierForMain(fishprops.grade, grade);
                    //            mass *= GetMassMultiplierForSecondary(subgrade_A);
                    //            tDef.SetStatBaseValue(StatDefOf.Mass, mass);

                    //            size *= GetSizeMultiplierForMain(fishprops.grade, grade);
                    //            size *= GetSizeMultiplierForSecondary(subgrade_B);
                    //            float nutrition = mass * size / 200;

                    //            tDef.SetStatBaseValue(StatDefOf.Nutrition, nutrition);
                    //            tDef.SetStatBaseValue(StatDefOf.MarketValue, nutrition * 10);
                    //            tDef.butcherProducts = new List<ThingDefCountClass>
                    //            {
                    //                new ThingDefCountClass(meat, (int)(nutrition / 0.04f))
                    //            };
                    //            tDef.comps.Add(new CompProperties_Forbiddable());

                    //            tDef.description = $"{grade} grade {item.label}. Its size is {size}cm and it weighs {mass}kg. Compared to other {item.label} of the same grade, it is {MassDesc(subgrade_A)} and {SizeDesc(subgrade_B)}";

                    //            tDef.modContentPack = LoadedModManager.RunningModsListForReading.FirstOrDefault(x => x.Name == "Fishing2");

                    //            tDef.ingestible = new IngestibleProperties
                    //            {
                    //                parent = tDef
                    //            };
                    //            IngestibleProperties ing = tDef.ingestible;
                    //            ing.foodType = FoodTypeFlags.Corpse;
                    //            ing.sourceDef = item;
                    //            ing.preferability = FoodPreferability.DesperateOnly;
                    //            //Log.Message($"{subgrade_A}{subgrade_B} 7 ", true);

                    //            ing.maxNumToIngestAtOnce = 1;
                    //            ing.ingestEffect = EffecterDefOf.EatMeat;
                    //            ing.ingestSound = SoundDefOf.RawMeat_Eat;
                    //            if (tDef.thingCategories == null)
                    //            {
                    //                tDef.thingCategories = new List<ThingCategoryDef>();
                    //            }

                    //            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(ing, "tasteThought", ThoughtDefOf.AteCorpse.defName);
                    //            DirectXmlCrossRefLoader.RegisterListWantsCrossRef(tDef.thingCategories, "TM_" + item.label + "_Category", nameof(HereWeGoooo));
                    //            //Log.Message($"{subgrade_A}{subgrade_B} 8 ", true);
                    //            //nutrition	=mass * size / 200

                    //            //1.08 = 3.6 * 60 /200
                    //            //60 cm
                    //            //  < statBases >
                    //            //  < Mass > 3.6 </ Mass >
                    //            //  < Nutrition > 1.08 </ Nutrition >
                    //            //  < MarketValue > 38 </ MarketValue >
                    //            //</ statBases >
                    //            //< butcherProducts >
                    //            //  < Meat_Flounder > 27 </ Meat_Flounder >
                    //            //</ butcherProducts >

                    //            yield return tDef;
                    //        }
                    //    }
                    //}
                    yield return meat;
                }
            }

            if (Prefs.DevMode)
                Log.Message($"FishGeneration generated { DefDatabase<ThingDef>.AllDefsListForReading.Count() - count} defs in {stopwatch.ElapsedMilliseconds} ms.");

            stopwatch.Stop();
        }

        private static bool Matches(ThingDef item, char[] Fishgrades)
        {
            char[] defName = item.defName.ToCharArray();

            if (defName.Length < 4)
                return false;
            if (defName[1] != '_')
                return false;
            if (!Fishgrades.Contains(defName[0]))
                return false;
            if (defName[2] != defName[3] || defName[4] != '_')
                return false;

            return item.HasModExtension<FishProperties>();
        }

        private static float GetMassMultiplierForMain(FishGrade fishGradeOne, FishGrade fishGradeTwo)
        {
            float result = 1f;

            if (fishGradeOne > fishGradeTwo)
                for (int i = 0; i < (fishGradeOne - fishGradeTwo); i++)
                {
                    result *= 1.28f;
                }
            else
                for (int i = 0; i < (fishGradeOne - fishGradeTwo); i++)
                {
                    result *= 0.72f;
                }
            return result;
        }

        private static float GetMassMultiplierForSecondary(FishSubGrade fishSubGrade)
        {
            //        F       D     C    B       A
            //Mass  x0.94   x0.97   1   x1.04   x1.1
            //Size  x0.95   x0.98   1   x1.03   x1.07
            switch (fishSubGrade)
            {
                case FishSubGrade.A:
                    return 1.1f;

                case FishSubGrade.B:
                    return 1.04f;

                case FishSubGrade.C:
                    return 1f;

                case FishSubGrade.D:
                    return 0.97f;

                case FishSubGrade.F:
                    return 0.94f;

                default:
                    return 1f;
            }
        }

        private static float GetSizeMultiplierForMain(FishGrade fishGradeOne, FishGrade fishGradeTwo)
        {
            float result = 1f;

            if (fishGradeOne > fishGradeTwo)
                for (int i = 0; i < (fishGradeOne - fishGradeTwo); i++)
                {
                    result *= 1.2f;
                }
            else
                for (int i = 0; i < (fishGradeOne - fishGradeTwo); i++)
                {
                    result *= 0.8f;
                }
            return result;
        }

        private static float GetSizeMultiplierForSecondary(FishSubGrade fishSubGrade)
        {
            //        F       D     C    B       A
            //Mass  x0.94   x0.97   1   x1.04   x1.1
            //Size  x0.95   x0.98   1   x1.03   x1.07
            switch (fishSubGrade)
            {
                case FishSubGrade.A:
                    return 1.07f;

                case FishSubGrade.B:
                    return 1.03f;

                case FishSubGrade.C:
                    return 1f;

                case FishSubGrade.D:
                    return 0.98f;

                case FishSubGrade.F:
                    return 0.95f;

                default:
                    return 1f;
            }
        }

        private static ThingDef MeatMaker(ThingDef source)
        {
            ThingDef d = new ThingDef
            {
                resourceReadoutPriority = ResourceCountPriority.Middle,
                category = ThingCategory.Item,
                thingClass = typeof(ThingWithComps),
                graphicData = new GraphicData
                {
                    graphicClass = typeof(Graphic_StackCount),
                    color = Color.magenta, //fancy! (for a placeholder)
                    //Things\Item\Fish
                    texPath = "Things/Item/Fish/Fishmeat"
                },
                useHitPoints = true,
                selectable = true
            };
            //Log.Message("1");
            d.SetStatBaseValue(StatDefOf.MaxHitPoints, 100f);
            d.altitudeLayer = AltitudeLayer.Item;
            d.stackLimit = 75;
            d.comps.Add(new CompProperties_Forbiddable());
            CompProperties_Rottable rotProps = new CompProperties_Rottable
            {
                daysToRotStart = 2f,
                rotDestroys = true
            };
            d.comps.Add(rotProps);
            //Log.Message("2");
            d.tickerType = TickerType.Rare;
            d.SetStatBaseValue(StatDefOf.Beauty, -4f);
            d.alwaysHaulable = true;
            d.rotatable = false;
            d.pathCost = 15;
            d.drawGUIOverlay = true;
            d.socialPropernessMatters = true;
            //Log.Message("3");
            d.modContentPack = source.modContentPack;
            //Log.Message("4");
            d.category = ThingCategory.Item;
            d.description = "MeatDesc".Translate(source.label);
            d.useHitPoints = true;
            d.SetStatBaseValue(StatDefOf.MaxHitPoints, 60f);
            d.SetStatBaseValue(StatDefOf.DeteriorationRate, 6f);
            d.SetStatBaseValue(StatDefOf.Mass, 0.03f);
            //Log.Message("5");
            d.SetStatBaseValue(StatDefOf.Flammability, 0.5f);
            d.SetStatBaseValue(StatDefOf.Nutrition, 0.05f);
            d.SetStatBaseValue(StatDefOf.FoodPoisonChanceFixedHuman, 0.02f);
            d.BaseMarketValue = 0.5f;
            //Log.Message("6");
            if (d.thingCategories == null)
            {
                d.thingCategories = new List<ThingCategoryDef>();
            }
            DirectXmlCrossRefLoader.RegisterListWantsCrossRef(d.thingCategories, "MeatRaw", d);
            //Log.Message("6");
            d.ingestible = new IngestibleProperties
            {
                parent = d,
                foodType = FoodTypeFlags.Meat,
                preferability = FoodPreferability.RawBad,
                ingestEffect = EffecterDefOf.EatMeat,
                ingestSound = SoundDefOf.RawMeat_Eat
            };
            //Log.Message("7");
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(d.ingestible, "tasteThought", ThoughtDefOf.AteRawFood.defName);

            d.uiIconPath = "Things/Item/Fishmeat";
            d.defName = "Meat_" + source.label;
            d.label = "MeatLabel".Translate(source.label);
            d.ingestible.sourceDef = source;
            //Log.Message("8");
            return d;
        }
    }
}
