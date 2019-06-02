using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;
using System.Reflection.Emit;

namespace WarCrimesExpanded
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("mehni.rimworld.warcrimesexpanded");

            harmony.Patch(AccessTools.Method(typeof(ITab_Pawn_Visitor), "FillTab"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(FillTabTranspiler)));
        }

        private static IEnumerable<CodeInstruction> FillTabTranspiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            foreach (var item in codeInstructions)
            {
                if (item.opcode == OpCodes.Ldc_R4 && 160f == (float)item.operand)
                {
                    item.opcode = OpCodes.Call;
                    item.operand = AccessTools.Method(typeof(HarmonyPatches), nameof(Height));
                }
                yield return item;
            }
        }

        private static float Height() => (DefDatabase<PrisonerInteractionModeDef>.DefCount * 30) + 10;
    }
}
