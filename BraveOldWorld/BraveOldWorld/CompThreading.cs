using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

//think mostly i'd appreciate a duplicate of the shearing comp/giver that relabels it as threading or something, and the wool growth to be silk growth instead. 

namespace BraveOldWorld
{
    public class CompThreading : CompShearable
    {
        protected override string SaveKey => "BOW_ShearGrowth";

        public override string ToString()
        {
            if (!Active)
            {
                return null;
            }
            return "BOW_ShearGrowth".Translate() + ": " + base.Fullness.ToStringPercent();
        }
    }
}
