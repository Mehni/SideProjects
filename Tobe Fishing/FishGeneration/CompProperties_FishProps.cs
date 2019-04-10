using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using TOBE_Fishing;

namespace FishGeneration
{
    public class CompProperties_FishProps : CompProperties
    {
        public FishGrade grade;
        public StatModifier mass;
        public StatModifier nutrition;

        public CompProperties_FishProps()
        {
            this.compClass = typeof(CompFishProps);
        }

        //public CompProperties_FishProps(FishGrade grade, FishSubGrade subGradeMass, FishSubGrade subGradeSize, float size, float mass, float nutrition)
        //{
        //    this.compClass = typeof(CompFishProps);

        //    this.grade = grade;
        //    this.subGradeMass = subGradeMass;
        //    this.subGradeSize = subGradeSize;
        //    this.size = size;
        //    this.mass = mass;
        //    this.nutrition = nutrition;
        //}
    }

    public class CompFishProps : ThingComp
    {
        public FishGrade    grade;
        public FishSubGrade subGradeMass;
        public FishSubGrade subGradeSize;
        public float        size = 60;
        private float massValue = 1f;
        private float nutritionalValue = 1f;
        public StatModifier mass = new StatModifier { stat = StatDefOf.Mass, value = 1 };
        public StatModifier nutrition = new StatModifier { stat = StatDefOf.Nutrition, value = 1 };

        public CompProperties_FishProps Props => (CompProperties_FishProps)this.props;

        public override string CompInspectStringExtra()
        {
            string text = "fishGrade".Translate() + ": " + grade;
            text = text + "\n" + "Size".Translate() + $" {size}" + "cm".Translate() + ".";
            text = text + "\n" + "Mass".Translate() + $" {this.parent.GetStatValue(StatDefOf.Mass)}" + "kg".Translate() + ".";

            return text;
        }

        public override string GetDescriptionPart()
        {
            //return $"{grade} grade {parent.def.label}. It's {size}cm long and weighs {this.parent.GetStatValue(StatDefOf.Mass)} {"kg".Translate()}. Compared to other {parent.def.label} of the same grade, it is {MassDesc(subGradeMass)} and {SizeDesc(subGradeSize)}.";

            return TranslatorFormattedStringExtensions.Translate("fishPropsDesc", grade.ToString(), parent.def.label, size, this.parent.GetStatValue(StatDefOf.Mass), "kg".Translate(), MassDesc(subGradeMass), SizeDesc(subGradeSize));
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.size,             "FishProps_size");
            Scribe_Values.Look(ref this.massValue,        "FishProps_mass");
            Scribe_Values.Look(ref this.nutritionalValue, "FishProps_nutrition");
            Scribe_Values.Look(ref this.subGradeMass,     "FishProps_subGradeMass");
            Scribe_Values.Look(ref this.subGradeSize,     "FishProps_subGradeSize");
            Scribe_Values.Look(ref this.grade,            "FishProps_grade");

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                this.mass = new StatModifier
                {
                    value = this.massValue,
                    stat = StatDefOf.Mass
                };
                this.nutrition = new StatModifier
                {
                    value = this.nutritionalValue,
                    stat = StatDefOf.Nutrition
                };
            }


        }

        private static string MassDesc(FishSubGrade grade)
        {
            switch (grade)
            {
                case FishSubGrade.A:
                    return "quiteHeavier".Translate();
                case FishSubGrade.B:
                    return "littlebitHeavier".Translate();
                case FishSubGrade.C:
                    return "averageWeight".Translate();
                case FishSubGrade.D:
                    return "littlebitLighter".Translate();
                case FishSubGrade.F:
                    return "quiteLighter".Translate();
                default:
                    return "very average";
            }
        }

        private static string SizeDesc(FishSubGrade grade)
        {
            switch (grade)
            {
                case FishSubGrade.A:
                    return "quiteBigger".Translate();
                case FishSubGrade.B:
                    return "littlebitBigger".Translate();
                case FishSubGrade.C:
                    return "averageSize".Translate();
                case FishSubGrade.D:
                    return "littlebitSmaller".Translate();
                case FishSubGrade.F:
                    return "quiteSmaller".Translate();
                default:
                    return "very average";
            }
        }
    }
}
