using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TOBE_Fishing
{
    public static class FishGradeUtility
    {
        public static List<FishGrade> allGrades = new List<FishGrade>();
        static FishGradeUtility()
        {
            foreach (FishGrade fishGrade in Enum.GetValues(typeof(FishGrade)))
            {
                allGrades.Add(fishGrade);
            }
        }

        public static FishGrade GenerateRandomGradeEqualChance()
        {
            return allGrades.RandomElement();
        }

        public static FishGrade GenerateFishGrade(ThingWithComps rod, Pawn pawn = null, float addsomeLevelsboys = 0)
        {

            if (pawn != null && !StatDefOf.HuntingStealth.Worker.IsDisabledFor(pawn))
                addsomeLevelsboys += pawn.GetStatValue(StatDefOf.HuntingStealth);

            if (rod.GetComp<CompQuality>() != null)
                addsomeLevelsboys += (float) rod.GetComp<CompQuality>().Quality / 6;

            if (rod.GetComp<CompFishingRod>() is CompFishingRod comp)
            {
                Log.Message($"rod, max: {comp.MaximumFishGrade()}, float: {addsomeLevelsboys}, min: {comp.MinimumFishGrade()} ");
                return GenerateFishGradeFromGaussianCurve(1,
                                                          comp.MaximumFishGrade(),
                                                          (FishGrade)((int)FishGrade.C + (int)addsomeLevelsboys),
                                                          comp.MinimumFishGrade());
            }
            Log.Message("no rod.");
            return GenerateFishGradeFromGaussianCurve(1);
        }

        public static FishGrade GenerateFishGradeFromGaussianCurve(float widthFactor, FishGrade max = FishGrade.S, FishGrade center = FishGrade.C, FishGrade min = FishGrade.F)
        {
            float num = Rand.Gaussian((float)center + 0.5f, widthFactor);
            if (num < (float)min)
            {
                num = (float)min;
            }
            if (num > (float)max)
            {
                num = (float)max;
            }
            return (FishGrade)(int)num;
        }
    }
}
