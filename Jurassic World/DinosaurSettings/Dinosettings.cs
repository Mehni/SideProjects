using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace DinosaurSettings
{
    public class DinoSettings : ModSettings
    {
        public static bool dinosCanSpawnWild = true;
        public static bool dinosCanBeReconstructed = true;

        public void DoWindowContents(Rect inRect)
        {
            Listing_Standard options = new Listing_Standard();
            Color defaultColor = GUI.color;

            options.Begin(inRect);

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = Color.yellow;
            options.Label("CHANGED SETTINGS NEED A GAMERESTART TO TAKE EFFECT. BLAME MEHNI.");
            GUI.color = defaultColor;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            options.Label("Settings");
            options.Gap();
            options.CheckboxLabeled("Life cannot be contained. Life breaks free. Life finds a way", ref dinosCanSpawnWild, "If disabled, no dinosaurs will come wandering on the map. You'll only be able to get dinos through reconstruction.");
            options.Gap();
            options.CheckboxLabeled("You bred raptors?", ref dinosCanBeReconstructed, "Your scientists were so preoccupied with whether or not they could, they didn’t stop to think if they should. \n\n Note: if disabled, no dino reconstruction.");

            options.End();

            Mod.GetSettings<DinoSettings>().Write();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref dinosCanSpawnWild, "dinosCanSpawnWild", true);
            Scribe_Values.Look(ref dinosCanBeReconstructed, "dinosCanBeReconstructed", true);
        }
    }

    public class DinosaurSettings : Mod
    {
        public DinoSettings settings;

        public DinosaurSettings(ModContentPack content) : base(content)
        {
            GetSettings<DinoSettings>();
        }

        public override string SettingsCategory() => "Jurassic RimWorld";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            GetSettings<DinoSettings>().DoWindowContents(inRect);
        }
    }
}
