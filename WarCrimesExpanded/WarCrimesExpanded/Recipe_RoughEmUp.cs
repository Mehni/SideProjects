using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace WarCrimesExpanded
{
    public class Recipe_RoughEmUp : Recipe_Surgery
    {
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(WCE_DefOf.WCE_TorturedMe, billDoer);

                if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);

                if (pawn.guest != null)
                {
                    ReduceResistance(billDoer, pawn);
                }
            }
            HealthUtility.GiveInjuriesOperationFailureMinor(pawn, part);
        }

        private static readonly SimpleCurve ResistanceImpactFactorCurve_Mood = new SimpleCurve
        {
            new CurvePoint(0f, 0.2f),
            new CurvePoint(0.5f, 1f),
            new CurvePoint(1f, 1.5f)
        };

        private static readonly SimpleCurve SurgerySuccessChanceCurve = new SimpleCurve
        {
            new CurvePoint(0.1f, 0.5f),
            new CurvePoint(0.5f, 1f),
            new CurvePoint(1.5f, 1.5f)
        };

        public void ReduceResistance(Pawn doc, Pawn patient)
        {
            float num = 1f;

            num *= ResistanceImpactFactorCurve_Mood.Evaluate(patient.needs.mood.CurLevelPercentage);
            num *= SurgerySuccessChanceCurve.Evaluate(doc.GetStatValue(StatDefOf.SurgerySuccessChanceFactor));

            float resistance = patient.guest.resistance;
            patient.guest.resistance = Mathf.Max(0f, patient.guest.resistance - num);

            string moteText = "TextMote_ResistanceReduced".Translate(resistance.ToString("F1"), patient.guest.resistance.ToString("F1"));

            if (patient.needs.mood?.CurLevelPercentage < 0.4f)
            {
                moteText = moteText + "\n(" + "lowMood".Translate() + ")";
            }
            if (doc.GetStatValue(StatDefOf.SurgerySuccessChanceFactor) < 0.3f)
            {
                moteText += "\n(Bad doctor)";
            }
            MoteMaker.ThrowText((doc.DrawPos + patient.DrawPos) / 2f, doc.Map, moteText, 8f);
            if (patient.guest.resistance == 0f)
            {
                string messageText = "MessagePrisonerResistanceBroken".Translate(patient.LabelShort, doc.LabelShort, doc.Named("WARDEN"), patient.Named("PRISONER"));
                if (patient.guest.interactionMode == PrisonerInteractionModeDefOf.AttemptRecruit)
                {
                    messageText = messageText + " " + "MessagePrisonerResistanceBroken_RecruitAttempsWillBegin".Translate();
                }
                Messages.Message(messageText, patient, MessageTypeDefOf.PositiveEvent);
            }
        }
    }
}
