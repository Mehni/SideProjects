using RimWorld;
using Verse;

namespace WarCrimesExpanded
{
    [DefOf]
    public static class WCE_DefOf
    {
        public static PrisonerInteractionModeDef WCE_TortureResistance;

        public static JobDef WCE_PrisonerTorture;

        public static ThoughtDef WCE_TorturedMe;

        static WCE_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(WCE_DefOf));
        }
    }
}
