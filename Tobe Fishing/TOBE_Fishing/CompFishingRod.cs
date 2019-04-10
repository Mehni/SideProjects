using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;


namespace TOBE_Fishing
{
    public class CompFishingRod : ThingComp
    {
        public CompProperties_FishingRod Props => (CompProperties_FishingRod)props;

        public FishGrade MinimumFishGrade() => Props.fishGrade.min;

        public FishGrade MaximumFishGrade() => Props.fishGrade.max;

        public FishGradeRange Range() => Props.fishGrade;
    }
}
