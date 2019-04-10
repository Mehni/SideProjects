using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace iLikeLamp
{
    public class CompProp_ChangedLampGlower : CompProperties_Glower
    {
        public CompProp_ChangedLampGlower()
        {
            compClass = typeof(PoweredByPhilipsHue);
        }
    }

    public class PoweredByPhilipsHue : CompGlower
    {
        int colourInt = -1;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var item in base.CompGetGizmosExtra())
            {
                yield return item;
            }
            yield return new Command_Action
            {
                defaultLabel = "Change colours",
                action = () =>
                         {
                             parent.Map.glowGrid.DeRegisterGlower(this);

                             if (!iLikeChangedLamps.COLOURS.ContainsKey(colourInt))
                                 colourInt = iLikeChangedLamps.COLOURS.First().Key;

                             props = iLikeChangedLamps.COLOURS[colourInt].comps.FirstOrDefault(x => x is CompProp_ChangedLampGlower);
                             colourInt++;

                             parent.Map.glowGrid.RegisterGlower(this);
                         },
                icon = ThingDefOf.StandingLamp.uiIcon,
                disabled = !LampResearch.ColoredLights.IsFinished,
                disabledReason = !LampResearch.ColoredLights.IsFinished ? $"Research {LampResearch.ColoredLights.label} first." : string.Empty,
            };
        }

        private string ColourAsString(ColorInt color) => $"RGBA({color.r}, {color.g}, {color.b}, {color.a})";

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref colourInt, "colourOfLamp");
        }
    }

    [StaticConstructorOnStartup]
    public static class iLikeChangedLamps
    {
        static iLikeChangedLamps()
        {
            int i = 0;
            foreach (ThingDef item in DefDatabase<ThingDef>.AllDefsListForReading.Where(x=> x.defName.StartsWith("StandingLamp")))
            {
                CompProperties_Glower compProps = (CompProperties_Glower)item.comps.FirstOrDefault(x => x is CompProperties_Glower);

                if (compProps == null)
                    continue;
                COLOURS.Add(i++, item);

                item.comps.Remove(compProps);
                CompProp_ChangedLampGlower compPropChangedLamp =
                    new CompProp_ChangedLampGlower
                    {
                        glowColor = compProps.glowColor,
                        glowRadius = compProps.glowRadius,
                        overlightRadius = compProps.overlightRadius
                    };
                item.comps.Add(compPropChangedLamp);
            }
        }

        public static readonly Dictionary<int, ThingDef> COLOURS = new Dictionary<int, ThingDef>();
    }

    [DefOf]
    public static class LampResearch
    {
        public static ResearchProjectDef ColoredLights;

        static LampResearch()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(LampResearch));
        }
    }
}
