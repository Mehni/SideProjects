using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Harmony;

namespace BraveOldWorld
{
    public class TheBroom : DeathActionWorker
    {
        public const int MAXLITTERS = 5;

        public override void PawnDied(Corpse corpse)
        {
            Hediff_Pregnant pregnant = (Hediff_Pregnant)corpse.InnerPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Pregnant, mustBeVisible: true);

            if (pregnant?.GestationProgress >= 0.8f)
            {
                for (int i = 0; i < MAXLITTERS; i++)
                {
                    Hediff_Pregnant.DoBirthSpawn(corpse.InnerPawn, pregnant.father);
                }
            }
        }
    }
}
