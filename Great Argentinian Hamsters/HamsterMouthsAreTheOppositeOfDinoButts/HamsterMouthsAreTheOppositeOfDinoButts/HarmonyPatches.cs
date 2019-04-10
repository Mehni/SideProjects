using System;
using Harmony;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace HamsterMouthsAreTheOppositeOfDinoButts
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("mehni.rimworld.likes.hamsters");
            HarmonyInstance.DEBUG = true;
            Log.Message("hello world");

            harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal", new Type[] { /*typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), */typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool) }), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(HamsterMouths)));

            harmony.Patch(AccessTools.Method(typeof(JobDriver), nameof(JobDriver.ModifyCarriedThingDrawPos)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(NOMNOMCHOOOMP)));
        }

        private static bool NOMNOMCHOOOMP(JobDriver __instance, ref bool __result, ref Vector3 drawPos)
        {
            //if instance is something with carrying, continue.
            if (!(__instance is JobDriver_TakeToBed || __instance is JobDriver_HaulToCell))
                return true;

            //if pawn has fat mouth, continue.
            if (!__instance.pawn.kindDef.HasModExtension<FatMouth>())
                return true;

            drawPos.x = -1000f;
            drawPos.y = -1000f;
            drawPos.z = -1000f;

            __result = true;

            return true;
        }

        private static void HamsterMouths(PawnRenderer __instance, Pawn ___pawn, Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump)
        {
            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 vector = rootLoc;

            //magic numbers!
            if (bodyFacing != Rot4.North)
            {
                vector.y += 0.0234375f;
            }
            else
            {
                vector.y += 0.02734375f;
            }

            if (!portrait && ___pawn.RaceProps.Animal && ___pawn.carryTracker?.CarriedThing != null && ___pawn.kindDef.GetModExtension<FatMouth>()?.FatMouthedHamster != null)
            {
                Mesh mesh = __instance.graphics.nakedGraphic.MeshAt(bodyFacing);
                Graphics.DrawMesh(mesh, vector, quaternion, ___pawn.kindDef.GetModExtension<FatMouth>().FatMouthedHamster.MatAt(bodyFacing), 0);
            }
        }
    }
}
    