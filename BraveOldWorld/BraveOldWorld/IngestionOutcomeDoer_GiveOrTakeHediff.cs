using RimWorld;
using Verse;

namespace BraveOldWorld
{
    public class IngestionOutcomeDoer_GiveOrTakeHediff : IngestionOutcomeDoer
    {
        //these 5 are set in XML or will have the below values (null for HediffDefs, -1 for the floats, false for the bool)
        public HediffDef hediffDefToTake;
        public HediffDef hediffDefToGive;

        public float severityToGive = -1f;
        public float severityToTake = -1f;

        public bool alwaysGiveSickness;

        //EXAMPLE XML
        //<li Class="BraveOldWorld.IngestionOutcomeDoer_GiveOrTakeHediff">
        //      <hediffDefToTake>NewWorldVenom</hediffDefToTake>
        //      <hediffDefToGive>SpiderSerumSickness</hediffDefToGive>
        //      <severityToGive>0.6</severityToGive>
        //      <alwaysGiveSickness>true</alwaysGiveSickness>
        //</li>

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            Hediff hediffToTake = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDefToTake);

            if (hediffToTake != null)
                AdjustOrRemoveHediff(hediffToTake, pawn);

            if (hediffToTake == null || alwaysGiveSickness)
                AddHediffToGiveTo(pawn);
        }

        private void AddHediffToGiveTo(Pawn pawn)
        {
            if (hediffDefToGive == null)
                return;

            Hediff sickness = HediffMaker.MakeHediff(hediffDefToGive, pawn);
            //use "severityToGive" from XML if it's greater than 0, otherwise use the (default) "initialSeverity" from the hediffDefToGive.
            float effect = severityToGive > 0f ? severityToGive : hediffDefToGive.initialSeverity;
            sickness.Severity = effect;
            pawn.health.AddHediff(sickness);
        }

        private void AdjustOrRemoveHediff(Hediff hediff, Pawn pawn)
        {
            if (severityToTake > 0.00f)
                hediff.Severity = severityToTake;
            else
                pawn.health.RemoveHediff(hediff);
        }
    }
}
