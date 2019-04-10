using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TOBE_Fishing
{
    using System.Xml;
    using JetBrains.Annotations;

    public class CompProperties_FishingRod : CompProperties
    {
        public FishGradeRange fishGrade;

        public CompProperties_FishingRod()
        {
            this.compClass = typeof(CompFishingRod);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }
        }
    }
}
