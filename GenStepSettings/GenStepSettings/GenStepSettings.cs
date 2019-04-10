using RimWorld;
using UnityEngine;
using Verse;

namespace GenStepSettings
{
    using JetBrains.Annotations;

    public class GenStepSettings : Mod
    {
        public readonly GenStepModSettings settings;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);
            string value = "";
            Widgets.IntEntry(inRect.RightPart(0.5f).BottomPart(0.5f), ref settings.count, ref value);
            GUI.EndGroup();
        }

        public override string SettingsCategory() => "GenStepSettings";

        public override void WriteSettings()
        {
            GenStep_Settlement genStep = (GenStep_Settlement)DefDatabase<GenStepDef>.GetNamed("Settlement").genStep;
            genStep.count = settings.count;
            base.WriteSettings();
        }

        public GenStepSettings(ModContentPack content) : base(content)
        {
            settings = GetSettings<GenStepModSettings>();
        }
    }

    public class GenStepModSettings : ModSettings
    {
        public int count;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref count, "count", 1);
        }
    }

    [StaticConstructorOnStartup]
    public class SettingsImplementer
    {
        static SettingsImplementer()
        {
            GenStep_Settlement genStep = (GenStep_Settlement)DefDatabase<GenStepDef>.GetNamed("Settlement").genStep;
            genStep.count = LoadedModManager.GetMod<GenStepSettings>().settings.count;
        }
    }
}
