using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;
using System.Reflection;
using System.Reflection.Emit;

namespace ClassLibrary1
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("mehni.rimworld.researchableDesignators");

            harmony.Patch(AccessTools.Property(typeof(DesignationCategoryDef), nameof(DesignationCategoryDef.ResolvedAllowedDesignators)).GetGetMethod(),
                          postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(DesignationCategory_ResolvedAllowed_PostFix)));

            //doing the reverse designator database didn't work. Fuck this.
            harmony.Patch(AccessTools.Method(typeof(InspectGizmoGrid), nameof(InspectGizmoGrid.DrawInspectGizmoGridFor)),
                          transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(FuckItDoATranspiler)));
        }

        private static IEnumerable<CodeInstruction> FuckItDoATranspiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeInstruction[] codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            for (int i = 0; i < codeInstructions.Length; i++)
            {
                CodeInstruction instr = codeInstructions[i];

                if (instr.opcode == OpCodes.Callvirt && instr.operand ==
                    AccessTools.Method(typeof(Designator), nameof(Designator.LabelCapReverseDesignating)))
                {
                    yield return codeInstructions[i - 5]; //action
                    yield return codeInstructions[i - 4];
                    yield return codeInstructions[i - 3]; //des
                    yield return codeInstructions[i - 2];
                    yield return codeInstructions[i - 1]; //thing

                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(DisableCommand)));
                }
                yield return instr;
            }
        }

        private static void DisableCommand(Command_Action action, Designator des, Thing t)
        {
            if (!(des is Designator_PlantsCut))
                return;
            if (!t.def.plant.IsTree)
                return;
            if (!ResearchProjectDefOf.CarpetMaking.IsFinished)
            {
                action.Disable("need to research woodcutting first!");
            }
        }

        private static void DesignationCategory_ResolvedAllowed_PostFix(ref IEnumerable<Designator> __result)
        {
            IEnumerable<Designator> designators = __result.Where(x => x is Designator_PlantsHarvestWood);
            foreach (Designator item in designators)
            {
                if (!ResearchProjectDefOf.CarpetMaking.IsFinished)
                    item.Disable("need to research woodcutting first!");
                else
                    item.disabled = false;
            }
        }
    }
}
