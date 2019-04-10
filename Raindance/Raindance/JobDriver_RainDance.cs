using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Harmony;
using Verse.AI;

namespace Raindance
{
    public class JobDriver_RainDance : JobDriver
    {
        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //toils are all evaluated at start of job, so putting some in delegates to guarantee a bit of randomness.
            Pawn pawn = this.GetActor();
            Toil bump = new Toil
            {
                initAction = () =>
                {
                    pawn.Drawer.Notify_MeleeAttackOn(TargetThingA);
                    

                }
            };
            Toil gotoRandom = new Toil
            {
                initAction = () =>
                {
                    Toils_Goto.GotoCell(TargetA.Cell.RandomAdjacentCell8Way(), PathEndMode.Touch);
                }
            };

            Toil makeItRain = new Toil
            {
                initAction = () =>
                {
                    //Toil does double duty for setting cooldown.
                    Building building = (Building)pawn.CurJob.targetA.Thing;
                    CompDancable compDancable = building.GetComp<CompDancable>();


                    //dancing for rain only works once a day
                    if ((Find.TickManager.TicksGame - compDancable.lastDanceTick) > 60000 || compDancable.lastDanceTick == -1)
                    {
                        //roll higher than 50% to get rain
                        //roll lower than a 10% chance to get... something else
                        float someDaysAreLuckierThanOthers = 0f;
                        if (GenDate.DaysPassed % 3 == 0)
                        {
                            someDaysAreLuckierThanOthers = Rand.Range(0.5f, 1f);
                        }

                        float value = Rand.Value;

                        if ((value + someDaysAreLuckierThanOthers) >= 0.5f) 
                        {
                            string text = pawn.Label + " " + "SuccesfullyDanced".Translate();
                            Messages.Message(text, new TargetInfo(pawn.Position, pawn.Map, false), MessageTypeDefOf.NeutralEvent);
                            pawn.Map.weatherManager.TransitionTo((DefDatabase<WeatherDef>.GetNamed("Rain", true)));
                        }
                        else if ((value + someDaysAreLuckierThanOthers) <= 0.05f)
                        {
                            //hurl chunks at the player
                            string text = pawn.Label + " " + "FailedDanceChunks".Translate();
                            Messages.Message(text, new TargetInfo(pawn.Position, pawn.Map, false), MessageTypeDefOf.NeutralEvent);
                            IncidentParms incidentParms0 = StorytellerUtility.DefaultParmsNow(Find.Storyteller.def, IncidentDefOf.ShipChunkDrop.category, pawn.Map);
                            IncidentDefOf.ShipChunkDrop.Worker.TryExecute(incidentParms0);
                        }
                        else if ((value + someDaysAreLuckierThanOthers) <= 0.075f)
                        {
                            string text = pawn.Label + " " + "FailedDanceThunder".Translate();
                            Messages.Message(text, new TargetInfo(pawn.Position, pawn.Map, false), MessageTypeDefOf.NeutralEvent);
                            pawn.Map.weatherManager.TransitionTo((DefDatabase<WeatherDef>.GetNamed("DryThunderstorm", true)));
                        }
                        else if ((value + someDaysAreLuckierThanOthers) <= 0.01f)
                        {
                            //hurl a tornado
                            string text = pawn.Label + " " + "FailedDanceTornado".Translate();
                            Messages.Message(text, new TargetInfo(pawn.Position, pawn.Map, false), MessageTypeDefOf.ThreatSmall);
                            IncidentParms incidentParms1 = StorytellerUtility.DefaultParmsNow(Find.Storyteller.def, DefDatabase<IncidentDef>.GetNamed("Tornado", true).category, pawn.Map);
                            DefDatabase<IncidentDef>.GetNamed("Tornado", true).Worker.TryExecute(incidentParms1);
                        }
                        else
                        {
                            string text = pawn.Label + " " + "FailedAppeaseGods".Translate();
                            Messages.Message(text, new TargetInfo(pawn.Position, pawn.Map, false), MessageTypeDefOf.NeutralEvent);
                        }
                        compDancable.DanceForATick(pawn);
                    }
                }
            };

            yield return Toils_Goto.Goto(TargetIndex.A, PathEndMode.Touch);
            yield return bump;

            yield return Toils_General.Wait(5);
            yield return Toils_Goto.GotoCell(TargetA.Cell.RandomAdjacentCell8Way(), PathEndMode.Touch);

            yield return Toils_General.Wait(33);
            yield return Toils_Goto.GotoCell(TargetA.Cell.RandomAdjacentCellCardinal(), PathEndMode.Touch);
            yield return gotoRandom;

            yield return Toils_General.Wait(15);
            yield return Toils_Goto.GotoCell(TargetA.Cell.RandomAdjacentCell8Way(), PathEndMode.Touch);
            yield return bump;

            yield return gotoRandom;
            yield return Toils_General.Wait(5);
            yield return Toils_Goto.GotoCell(TargetA.Cell.RandomAdjacentCellCardinal(), PathEndMode.Touch);

            yield return Toils_General.Wait(18);
            yield return Toils_Goto.GotoCell(TargetA.Cell.RandomAdjacentCell8Way(), PathEndMode.Touch);
            yield return bump;

            yield return Toils_General.Wait(26);
            yield return Toils_Goto.GotoCell(TargetA.Cell.RandomAdjacentCellCardinal(), PathEndMode.Touch);
            yield return gotoRandom;

            yield return Toils_General.Wait(34);
            yield return Toils_Goto.GotoCell(TargetA.Cell.RandomAdjacentCell8Way(), PathEndMode.Touch);
            yield return bump;

            yield return Toils_General.Wait(15);
            yield return gotoRandom;
            yield return Toils_Goto.GotoCell(TargetA.Cell.RandomAdjacentCellCardinal(), PathEndMode.Touch);
            yield return bump;

            yield return Toils_General.Wait(52);
            yield return Toils_Goto.GotoCell(TargetA.Cell.RandomAdjacentCell8Way(), PathEndMode.Touch);

            yield return Toils_General.Wait(5);
            yield return makeItRain;
            yield break;
        }
    }

    [DefOf]
    public static class RaindanceDefOf
    {
        public static JobDef RainDance;
    }
}
