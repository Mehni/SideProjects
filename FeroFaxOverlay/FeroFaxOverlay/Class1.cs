using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Harmony;

namespace FeroFaxOverlay
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("Mehni.RimWorld.FeroFaxQuickAndDirtyOverlay");

            harmony.Patch(AccessTools.Method(typeof(PawnUIOverlay), nameof(PawnUIOverlay.DrawPawnGUIOverlay)), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(AddABigUglyX_PostFix)), null);
        }

        private static void AddABigUglyX_PostFix(PawnUIOverlay __instance, Pawn ___pawn)
        {
            if (___pawn.RaceProps.Humanlike) return;

            if (___pawn.Spawned && !___pawn.Map.fogGrid.IsFogged(___pawn.Position) &&___pawn.IsEnemyOfPlayer())
                ___pawn.Map.overlayDrawer.DrawOverlay(___pawn, OverlayTypes.ForbiddenBig);
        }

        private static bool IsEnemyOfPlayer(this Pawn pawn) => pawn.InAggroMentalState || (pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer));
    }
}
